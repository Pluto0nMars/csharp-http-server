# HTTP Server

## Overview

For fun, I decided to explore threading and asynchronous processes in C#. I was mainly inspired by my systems programing class ,
(CS 361 @ UIC), which explored threads concurrent processes/methods that occur with servers in C. I found the project to be interesting so I decided
to see what an HTTP server would look like in C#. The server can be ran locally and interacted with through either a browser or terminal.

<img width="703" height="579" alt="image" src="https://github.com/user-attachments/assets/e8d09500-2010-497f-8679-a3891063ecb9" />



## How it Works
Here's the the data flow of the server:
<img width="720" height="720" alt="image" src="https://github.com/user-attachments/assets/fa562474-41da-43ee-a6dc-8778d4e6ab10" />


## Browser interaction
<img width="1358" height="673" alt="image" src="https://github.com/user-attachments/assets/037a3566-3a27-4f6e-935f-e72657d34ee7" />


Typing directly into the browser address bar sends an `HTTP GET` request. Navigate to any of the URLs listed. Each `url` is a different method for the server interaction. The server needs to running in the background of course, but if someone prefers, they could interact with the server and place all of their request through the browser. In order for the browser to give  the user the ability to perform methods directly on the home route, browsers require explicit instructions via JavaScript `fetch()`. Responses are given in `JSON` format for consistency with the terminal output.

Alternatively you could past each request directly into the address bar, but I think that the home route offers a more convenient experience if you prefer the browser. I trade off of using the browser as opposed to the terminal would be the fact that post requests are slight more limited as you cannot directly decide the content of the `POST` request where as in the terminal this is not an issue. I hope to address this in the future so that both options have same level of user autonomy. 





