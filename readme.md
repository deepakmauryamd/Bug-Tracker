clone project
run dotnet restore
dotnet build
func + F5 to launch on browser

to run migrations
dotnet ef migrations add InitialCreate

to update database
dotnet ef database update


User Id and password with admin role which can add/remove roles from/to users
username: deepak
password: deepak123


To Add Project Manager role is required
Admin can give manager role

logout after assigning role and login again to see changes/access

other role: Developer
username: sanjay
password: sanjay123

username: anirudh
password: anirudh123


Current Issues:
No link to reach to projects page? (/AllBugs/{projectId}/) : eg. /AllBugs/1/
Add bug not working