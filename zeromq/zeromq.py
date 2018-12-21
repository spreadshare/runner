import zmq
import time
import sys
import os
from time import sleep
def file_get_contents(filename):
  if os.path.exists(filename):
    fp = open(filename, "r")
    content = fp.read()
    fp.close()
    return content

# Get json files
from os import listdir
from os.path import isfile, join
mypath = "./json/"
files = [f for f in listdir(mypath) if isfile(join(mypath, f))]

# ZeroMQ context
context = zmq.Context()

# Request-reply
print("Testing req-rep")
socket = context.socket(zmq.REQ)

print("Connecting to server...")
socket.connect("tcp://spreadshare:5555")

for file in files:
  print("\nStarting test: " + file)
  socket.send((file_get_contents(mypath + file).encode('utf-8')))
  message = socket.recv()
  print("Received reply ", "[", message, "]")


# Subscribing
print("Testing pub-sub")
subscriber = context.socket(zmq.SUB)
subscriber.connect("tcp://spreadshare:5556")
subscriber.setsockopt(zmq.SUBSCRIBE, b"topic_status")
subscriber.setsockopt(zmq.SUBSCRIBE, b"topic_holdtime")

while True:
    topic, data = subscriber.recv_multipart()
    topic = topic.decode('utf-8')
    assert topic == "topic_status" or topic == "topic_holdtime"
    print(data)