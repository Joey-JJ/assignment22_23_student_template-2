# Setup and run

To run the test-server to test your client implementation.

## Install

Go to the website of docker and install it on your machine. Once docker installed and started follow those
steps to connect your client to the server.

## Step 1: Load

<!-- navigate in your command line to directory where the .tar image is stored -->
<!-- type the following command in your command line to load the image-->

docker load < file_download_server_image.tar

## Step 2: Check

<!-- type the following command in your command line to see if the image is now enlisted. Your docker image name must match with the image name of the command in step 3. If you are using docker desktop you should be able to see it in the images section  -->

docker image ls

## Step 2: Run

<!-- run the image as a container that listens to UPD protocol on port 5004. Be aware the UDP must be included in the command otherwise it will listen to TCP by default and the server will not communicate with your client -->

docker run -i -t -p 5004:5004/udp file_download_server-image:latest
