import csv
import psycopg2
import os
import re
import logging
from datetime import datetime
from decimal import Decimal
from os import listdir
from os.path import isfile, join
from time import sleep
from urllib.parse import urlparse

PATH = "./input-data"
EXTENSION = ".csv"


class DataPump:
    def __init__(self):
        self.__logger = self.init_logging()

        files = self.get_csv_files(PATH, EXTENSION)

        # Check if there are any files
        if len(files) == 0:
            self.__logger.warning(f"No data found. Please place your csv files in {PATH}. Exiting now")
            exit(0)

        self.__logger.info("Pushing data of the following csv files:")
        for f in files:
            self.__logger.info("\t" + join(PATH, f + EXTENSION))

        for file in files:
            jobs = self.fetch_import_jobs(PATH, EXTENSION, file)
            if jobs is not None:
                self.insert_data(jobs, file)

    def init_logging(self):
        logging.basicConfig()
        logger = logging.getLogger(self.__class__.__name__)
        logger.setLevel(logging.INFO)
        logger.propagate = False

        if not len(logger.handlers):
            ch = logging.StreamHandler()
            ch.setLevel(logging.INFO)
            formatter = logging.Formatter('%(message)s')
            ch.setFormatter(formatter)
            logger.addHandler(ch)

        return logger

    @staticmethod
    def get_csv_files(path, extension):
        return [f[:-len(".csv")] for f in listdir(path) if (isfile(join(path, f)) and f.endswith(extension))]

    def fetch_import_jobs(self, path, extension, file):
        job_list = []

        with open(join(path, file + extension), "r") as f:
            reader = csv.reader(f, delimiter=",", quotechar="|")

            # Check if header
            sniffer = csv.Sniffer()
            has_header = sniffer.has_header(f.read(2048))
            f.seek(0)

            # Skip header
            if has_header:
                h = next(reader)
                self.__logger.warning(f"{file}: Expected header: \tTimestamp, Open, Close, High, Low, Volume")
                self.__logger.warning(f"{file}: Given header:\t\t{','.join(h)}")
            else:
                self.__logger.warning(f"{file}: Expected header: \tTimestamp, Open, Close, High, Low, Volume")
                self.__logger.warning(f"{file}: No header given!")

            # Fix formatting
            prev_nr = -1

            for row in reader:
                # Remove spaces and tabs
                row = [re.sub(r"(^[ \t]+|[ \t]+(?=:))", "", x, flags=re.M) for x in row]

                # Convert numbers to floats
                row = [float(x) for x in row]
                if prev_nr != -1 and prev_nr + 60000 != row[0] and prev_nr - 60000 != row[0]:
                    self.__logger.critical(f"{file}: Timestamp error at: {row[0]}. Import blocked")
                    return None
                prev_nr = row[0]

                row.append(file)
                job_list.append(tuple(row))

        return job_list

    def insert_data(self, jobs, file):
        conn = self.get_connection()
        cur = conn.cursor()

        # Insert data
        start = datetime.now()
        self.__logger.info(f"{file}: Start insertion of " + str(len(jobs)) + " jobs")
        sql = """INSERT INTO "Candles" ("Timestamp", "Open", "Close", "High", "Low", "Volume", "TradingPair")
               VALUES ({0},{1},{2},{3},{4},{5},'{6}')"""
        for job in jobs:
            try:
                cur.execute(sql.format(*job))
            except psycopg2.IntegrityError:
                self.__logger.error(f"{file}: ForeignKeyConstraint; skipping job\n")
                conn.close()
                return
            except Exception as ex:
                self.__logger.error(f"{file}: Unexpected exception occurred:\n{ex}\n")
                conn.close()
                return

        conn.commit()
        conn.close()
        self.__logger.info(f"{file}: Finished insertion\nElapsed time: {datetime.now() - start}")

    def get_connection(self):
        # Parse parameters and connect
        try:
            url = urlparse(os.environ["DATABASE_URL"])
        except KeyError as e:
            self.__logger.error(e)
            self.__logger.error(
                "Could not find environmental variable 'DATABASE_URL'. Did you copy .env.example to .env?"
            )

        retries = 0
        max_attempts = 10

        while True:
            try:
                conn = psycopg2.connect(
                    database=url.path[1:],
                    user=url.username,
                    password=url.password,
                    host=url.hostname,
                    port=url.port,
                )
                break
            except psycopg2.OperationalError as e:
                retries += 1
                if retries > max_attempts:
                    self.__logger.error(f"Maximum attempts ({max_attempts}) reached. Stopping program")
                    exit(1)
                self.__logger.warning(e)
                self.__logger.warning("Could not connect to database! Sleeping 5 seconds")
                sleep(5)

        return conn


def add_coloring_to_emit_windows(fn):
    # add methods we need to the class
    def _out_handle(self):
        import ctypes
        return ctypes.windll.kernel32.GetStdHandle(self.STD_OUTPUT_HANDLE)

    out_handle = property(_out_handle)

    def _set_color(self, code):
        import ctypes
        # Constants from the Windows API
        self.STD_OUTPUT_HANDLE = -11
        hdl = ctypes.windll.kernel32.GetStdHandle(self.STD_OUTPUT_HANDLE)
        ctypes.windll.kernel32.SetConsoleTextAttribute(hdl, code)

    setattr(logging.StreamHandler, '_set_color', _set_color)

    def new(*args):
        FOREGROUND_BLUE = 0x0001  # text color contains blue.
        FOREGROUND_GREEN = 0x0002  # text color contains green.
        FOREGROUND_RED = 0x0004  # text color contains red.
        FOREGROUND_INTENSITY = 0x0008  # text color is intensified.
        FOREGROUND_WHITE = FOREGROUND_BLUE | FOREGROUND_GREEN | FOREGROUND_RED
        # winbase.h
        STD_INPUT_HANDLE = -10
        STD_OUTPUT_HANDLE = -11
        STD_ERROR_HANDLE = -12

        # wincon.h
        FOREGROUND_BLACK = 0x0000
        FOREGROUND_BLUE = 0x0001
        FOREGROUND_GREEN = 0x0002
        FOREGROUND_CYAN = 0x0003
        FOREGROUND_RED = 0x0004
        FOREGROUND_MAGENTA = 0x0005
        FOREGROUND_YELLOW = 0x0006
        FOREGROUND_GREY = 0x0007
        FOREGROUND_INTENSITY = 0x0008  # foreground color is intensified.

        BACKGROUND_BLACK = 0x0000
        BACKGROUND_BLUE = 0x0010
        BACKGROUND_GREEN = 0x0020
        BACKGROUND_CYAN = 0x0030
        BACKGROUND_RED = 0x0040
        BACKGROUND_MAGENTA = 0x0050
        BACKGROUND_YELLOW = 0x0060
        BACKGROUND_GREY = 0x0070
        BACKGROUND_INTENSITY = 0x0080  # background color is intensified.

        levelno = args[1].levelno
        if (levelno >= 50):
            color = BACKGROUND_YELLOW | FOREGROUND_RED | FOREGROUND_INTENSITY | BACKGROUND_INTENSITY
        elif (levelno >= 40):
            color = FOREGROUND_RED | FOREGROUND_INTENSITY
        elif (levelno >= 30):
            color = FOREGROUND_YELLOW | FOREGROUND_INTENSITY
        elif (levelno >= 20):
            color = FOREGROUND_GREEN
        elif (levelno >= 10):
            color = FOREGROUND_MAGENTA
        else:
            color = FOREGROUND_WHITE
        args[0]._set_color(color)

        ret = fn(*args)
        args[0]._set_color(FOREGROUND_WHITE)
        # print "after"
        return ret

    return new


def add_coloring_to_emit_ansi(fn):
    # add methods we need to the class
    def new(*args):
        levelno = args[1].levelno
        if (levelno >= 50):
            color = '\x1b[31m'  # red
        elif (levelno >= 40):
            color = '\x1b[31m'  # red
        elif (levelno >= 30):
            color = '\x1b[33m'  # yellow
        elif (levelno >= 20):
            color = '\x1b[32m'  # green
        elif (levelno >= 10):
            color = '\x1b[35m'  # pink
        else:
            color = '\x1b[0m'  # normal
        args[1].msg = color + args[1].msg + '\x1b[0m'  # normal
        # print "after"
        return fn(*args)

    return new


import platform

if platform.system() == 'Windows':
    # Windows does not support ANSI escapes and we are using API calls to set the console color
    logging.StreamHandler.emit = add_coloring_to_emit_windows(logging.StreamHandler.emit)
else:
    # all non-Windows platforms are supporting ANSI escapes so we use them
    logging.StreamHandler.emit = add_coloring_to_emit_ansi(logging.StreamHandler.emit)

if __name__ == "__main__":
    DataPump()
