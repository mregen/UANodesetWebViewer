# UANodesetWebViewer
An OPC UA nodeset XML file viewer running in a webpage. The webpage can be hosted within a Docker container. You can load nodeset files and then browse them. Super handy if you want to look at the standardized nodeset files defined in the OPC UA companion specs.

## Usage

It is published on DockerHub: https://hub.docker.com/r/barnstee/uanodesetwebviewer

Run it via: docker run barnstee/uanodesetwebviewer:latest

###  Upload 

![Start](docs/Start.png)

- Upload your XML NodeSet file. (NOTE: Dependent NodeSet files need to be uploaded together or in the required order.)


### Browsing

![Browsing](docs/Sample.png)

- One can browse and interact with the model.
- Currently `READ` and `WRITE` of a node is possible.
