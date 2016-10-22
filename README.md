# SFWorkshopTwo
This is the second workshop in a series of 3. This workshop focuses on having a front end web server and back end stateful service to showcase how to work with Reliable Services.

The stateful service back-end automatically increments a counter once every minute to showcase a background running process. The front end exposes the ability to read that counter or post a message to increment the counter outside of the background running process.

API Examples:

api/cameras using GET     Returns the count of the cameras

api/cameras using POST    Increments the camera counter by one

/ (Root of Website)       Brings up a web page that has a button that will invoke the api/cameras POST. A tool such as POSTMAN can be used as well.

