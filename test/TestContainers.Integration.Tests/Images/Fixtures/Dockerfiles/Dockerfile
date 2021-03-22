FROM busybox:latest

# test context passing
COPY . /www

EXPOSE 80
HEALTHCHECK CMD nc -z localhost 80

# Create a basic webserver and run it until the container is stopped
CMD httpd -p 80 -h /www -f & wait
