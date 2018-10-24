FROM python:3.6-slim

RUN apt-get update 
RUN apt-get -y upgrade
RUN apt-get -y install -q build-essential
RUN apt-get -y install -q python-dev libffi-dev libssl-dev python-pip apt-utils
RUN apt-get -y install -q gcc musl-dev
RUN apt-get -y install -q libpq-dev
RUN pip install psycopg2

# Set the working directory to /app
WORKDIR /app

# Copy the current directory contents into the container at /app
COPY . /app