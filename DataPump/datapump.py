import csv
import psycopg2
import os
import re
from datetime import datetime
from decimal import Decimal
from os import listdir
from os.path import isfile, join
from time import sleep
from urllib.parse import urlparse

def main():
  path = "./data"
  extension = ".csv"
  files = getCSVFiles(path, extension)
  printStatusUpdate("Pushing data of the following csv files:"
    + "".join(["\n\t" + join(path, f + extension) for f in files]))
  jobs = convertFilesToJobs(path, files, extension)
  insertData(jobs)

def printStatusUpdate(message = None):
  print("##############")
  if not(message is None):
    print(message)
    print("##############")

def getCSVFiles(path, extension):
  return [f[:-4] for f in listdir(path) if (isfile(join(path, f)) and f.endswith('.csv'))]


def convertFilesToJobs(path, files, extension):
  joblist = []

  for file in files:
    with open (join(path, file + extension), 'r') as f:
      reader = csv.reader(f, delimiter=',', quotechar='|')

      # Check if header
      sniffer = csv.Sniffer()
      has_header = sniffer.has_header(f.read(2048))
      f.seek(0)

      # Skip header
      if (has_header):
        h = next(reader)
        printStatusUpdate()
        print("Expected header: \t\tTimestamp, Open, Close, High, Low, Volume")
        print(f"{file}: Given header:\t{','.join(h)}")
        printStatusUpdate()
      else:
        printStatusUpdate()
        print("Expected header: \t\tTimestamp, Open, Close, High, Low, Volume")
        print(f"{file}: No header given!")
        printStatusUpdate()

      # Fix formatting
      for row in reader:
        # Remove spaces and tabs
        row = [re.sub(r'(^[ \t]+|[ \t]+(?=:))', '', x, flags=re.M) for x in row]
        
        # Convert numbers to floats
        row = [float(x) for x in row]
        row.append(file)
        joblist.append(tuple(row))

  return joblist


def insertData(jobs):
  conn = getConnection()
  cur = conn.cursor()

  # Insert data
  start = datetime.now()
  printStatusUpdate("Start insertion of " + str(len(jobs)) + " jobs")
  sql = """INSERT INTO "Candles" ("Timestamp", "Open", "Close", "High", "Low", "Volume", "TradingPair")
           VALUES ({0},{1},{2},{3},{4},{5},'{6}')"""
  for job in jobs:
    cur.execute(sql.format(*job))

  conn.commit()
  conn.close()
  printStatusUpdate(f"Finished insertion\nElapsed time: {datetime.now() - start}")


def getConnection():
  # Parse parameters and connect
  try:
    url = urlparse(os.environ["DATABASE_URL"])
  except KeyError as e:
    print(e)
    print("Could not find environmental variable 'DATABASE_URL'. Did you copy .env.example to .env?")

  retries = 0
  max_attempts = 10

  while True:
    try:
      conn = psycopg2.connect(
          database=url.path[1:],
          user=url.username,
          password=url.password,
          host=url.hostname,
          port=url.port)
      break
    except psycopg2.OperationalError as e:
      retries += 1
      if retries > max_attempts:
        print(f"Maximum attempts ({max_attempts}) reached. Stopping program")
        exit()
      print(e)
      print("Could not connect to database! Sleeping 5 seconds")
      sleep(5)

  return conn


if __name__ == "__main__":
  main()