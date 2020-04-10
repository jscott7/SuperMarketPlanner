FROM resin/rpi-raspbian:latest
ENTRYPOINT []

RUN apt-get -q update && \
    apt-get -qy install \
    python python-pip 

RUN pip2 install more-itertools==2.6
RUN pip2 install jaraco.functools==2.0
RUN pip2 install web.py==0.51

WORKDIR /app

COPY RestServerSSL.py /app

# I want to setup a data volume instead of copying to the image
# COPY data /home/pi/data/ 

COPY Certificates /app/Certificates

CMD ["python", "RestServerSSL.py", "8443"]
