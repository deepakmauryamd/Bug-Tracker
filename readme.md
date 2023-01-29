# Bug Tracking Web Application
It's a web application designed to manage and track bugs of different projects in an organization. It gives single view of all the projects, currently opened bugs and total resolved bugs. This project uses `dotnet core 6.0`, `razor c#`, `Entity Framework Core`, `Identity Core` and `MySQL`.

#### It has 3 access roles `Admin`, `Manager` and `Developer`. 
- Developer can view all the projects and bugs in each project and also `add`, `edit`, `resolve`, `delete` bugs in the project.
- Manager can `add Project` as well as all the access which developer has.
- Admin can `add roles` and `manager users`.

## How to run this project locally
- Clone the project and open it in a code editor.
- Open terminal, change directory to root of the project if already not there and run below command:-
```
dotnet restore
```
- After successful completion of above command, run:-
```
dotnet build
```
Or
- Can press `func + F5` to launch the browser, and it will show a login screen.

- To Create database and tables, you can run below command:-
```
dotnet ef migrations add Init
```

and after successful completion on above command, run:-

```
dotnet ef database update
```

- To Create Users and Roles with some default data, you can run `seedData.sql` script, it will create all the roles and default user to use the application.

- Also, you can sign up with UserName and Password, it will log in with default role as developer

## Project Screens with Admin role because it has all the access:-
### Dashboard
![Project Overview Dashboard](https://github.com/maurya-deepak/Bug-Tracker/blob/master/ReadmeImages/home-screen.png?token=GHSAT0AAAAAAB5ALC5Y7L2ZWPQF2XWKBF2WY6WVHWA)
### Add Project Screen
![Add Project Screen](https://github.com/maurya-deepak/Bug-Tracker/blob/master/ReadmeImages/add-project-screen.png?token=GHSAT0AAAAAAB5ALC5YSKLGJWVVQJEV34KQY6WVN5A)
### All Bugs of a Project Dashboard
![All bugs Dashboard](https://github.com/maurya-deepak/Bug-Tracker/blob/master/ReadmeImages/all-bugs-screen.png?raw=true)
### Add Bug Screen
![Add Bug Screen](https://github.com/maurya-deepak/Bug-Tracker/blob/master/ReadmeImages/add-bug-screen.png?token=GHSAT0AAAAAAB5ALC5YQ4AKCWO2F4JLUFDQY6WVNIA)
