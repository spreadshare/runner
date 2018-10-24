import os

from urllib.parse import urlparse

import psycopg2

url = urlparse(os.environ["DATABASE_URL"])

conn = psycopg2.connect(
    database=url.path[1:],
    user=url.username,
    password=url.password,
    host=url.hostname,
    port=url.port
)

print(url.path[1:])

cur = conn.cursor()
cur.execute("SELECT version();")
print(cur.fetchone())

cur.execute("select relname from pg_class where relkind='r' and relname !~ '^(pg_|sql_)';")
print(cur.fetchall())

# sql = """INSERT INTO "Candles" ("Timestamp", "Close", "High", "Low", "Open", "TradingPair", "Volume")
#          VALUES (2,2,3,4,5,6, 'd');"""
# timestamp = None
# cur.execute(sql)
# timestamp = cur.fetchone()[0]
# conn.commit()

sql = """ SELECT * FROM "Candles" """
cur.execute(sql)
print(cur.fetchall())

conn.close()
