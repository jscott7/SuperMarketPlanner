FROM resin/rpi-raspbian:latest
ENTRYPOINT []

RUN apt-get -q update && \
    apt-get -qy install \
    python python-pip 

RUN pip install web.py

WORKDIR /app

COPY RestServer.py /app

COPY data /home/pi/data/ 

COPY templates /home/pi/templates

CMD ["python", "RestServer.py"]
