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

Alternatively you could past each request directly into the address bar, for example : `http://localhost:8080/api/users`, but I think that the home route offers a more convenient experience if you prefer the browser. I trade off of using the browser as opposed to the terminal would be the fact that post requests are slight more limited as you cannot directly decide the content of the `POST` request where as in the terminal this is not an issue. I hope to address this in the future so that both options have same level of user autonomy. 

## Performance
In my program, I used C#'s built-in System.Diagnostics and PeriodicTimer libraries to record the Server Performance Statistics every 5 seconds. The statistics included: 

* Total Requests / second
* Server Speed (Requests / second)

<img width="600" height="371" alt="Total Requests vs  Seconds" src="https://github.com/user-attachments/assets/bb2f6ddc-0e14-401a-b2ee-9148d194bb5d" />
<br/>
<br/>
<img width="600" height="371" alt="Requests per Second vs  Seconds" src="https://github.com/user-attachments/assets/c6f0d849-555f-4716-b942-47faf2eba45d" />


## Experience
I chose to use simple technologies  in order to gain a better understanding of how a server would operate in C#. Additional libraries were limited to C#'s built in Threading and Threading.Tasks for concurrent handling of server requests and responses. I also added the Diagnostics to perform performance testing.  All other operations including creating the HTML home route, displaying various Output, and benchmarking were done using C#'s standard library. This approach allowed me to prioritize the main methods associated with an HTML server while also allowing me to easily expand the available methods in the future.

I was familiar with Async/Await and Task before I started the project, but after completing server I have a better perspective when it comes to how it can be utilized in other applications, particularly ones involving concurrent processes. The error handling associated with these keywords was also a great learning experience when it came to understanding the various exception that make occur while running the server. It was important to handle the errors gracefully so as to not crash the server due to one user's error.




