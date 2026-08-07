# Writing a thirtytwo Application

These guides explain how to create a thirtytwo desktop application, run its UI
message loop, and safely schedule work on the UI thread.

## Guides

1. [Creating and running an Application](creating-and-running-an-application.md)
   describes the executable project, root window, message handling, application
   lifetime, and window ownership.
2. [Dispatching to the UI Thread](dispatching.md) describes how to find a UI
   dispatcher, queue synchronous or asynchronous work, use cancellation and
   delays, create timers, optimize thread checks, and handle failures and
   shutdown.

The complete sample applications under [`src/samples`](../src/samples/) provide
larger examples of windows, controls, graphics, dialogs, and layout.