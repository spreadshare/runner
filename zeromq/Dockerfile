# Use an official Python runtime as a parent image
FROM python:3.6-slim

RUN apt-get update 
RUN apt-get -y upgrade 
RUN apt-get -y install -q build-essential
RUN apt-get -y install -q python-dev libffi-dev libssl-dev python-pip apt-utils

# Install any needed packages specified in requirements.txt
RUN pip install --trusted-host pypi.python.org zmq

# Set the working directory to /app
WORKDIR /app

# Copy the current directory contents into the container at /app
COPY . /app