import os
import re
import csv
import psycopg2
from urllib.parse import urlparse
from decimal import Decimal


from os import listdir
from os.path import isfile, join
onlyfiles = [f for f in listdir("./data") if isfile(join("./data", f))]
print(onlyfiles)

joblist = []



with open (onlyfiles[0], 'r') as file:
  reader = csv.reader(file, delimiter=',', quotechar='|')
  next(reader, None)
  for row in reader:
    row = [re.sub(r'(^[ \t]+|[ \t]+(?=:))', '', x, flags=re.M) for x in row]
    row = [float(x) for x in row]
    row.append("BNBETH")
    joblist.append(tuple(row))

print(joblist)





url = urlparse(os.environ["DATABASE_URL"])
conn = psycopg2.connect(
    database=url.path[1:],
    user=url.username,
    password=url.password,
    host=url.hostname,
    port=url.port
)

cur = conn.cursor()

sql = """INSERT INTO "Candles" ("Timestamp", "Close", "High", "Low", "Open", "Volume", "TradingPair") VALUES ({0},{1},{2},{3},{4},{5},'{6}')"""
for i in range(124,1000000):
  job = (i, 0,0,0,0,0,"adf")
  cur.execute(sql.format(*job))

# sql = """INSERT INTO "Candles" ("Timestamp", "Close", "High", "Low", "Open", "Volume", "TradingPair")
#          VALUES (123,2,3,4,5,6, 'd');"""
# timestamp = None
# cur.execute(sql, 0,0,0,0,0,'b')
#timestamp = cur.fetchone()
conn.commit()

conn.close()