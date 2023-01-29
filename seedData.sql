insert into BugTracker.AspNetRoles(Id, Name, NormalizedName, ConcurrencyStamp) 
Values('fab4fac1-c546-41de-aebc-a14da6895711', 'Admin', 'ADMIN', '1'),
('fab4fac1-c546-41de-aebc-a14da6895712', 'Manager', 'Manager', '3'),
('fab4fac1-c546-41de-aebc-a14da6895713', 'Developer', 'Developer', '3');

insert into AspNetUsers
values('319871ae-62fb-448b-bb9d-e7b4166310uf', 'ApplicationUser', 'admin', 'ADMIN', 'admin@gmail.com', 'ADMIN@GMAIL.COM', '0', 'AQAAAAEAACcQAAAAEED9Idc5Dm7fck+FsEQj6w89qwt0dgAtWgOO1lX2V9i4RU46x/ku8KcsFC5unhhAZQ==', 'NCK4PT3ZKXTOS6SU64AJ65A5GDY55JI6', '5f002799-6e22-4af9-83ef-950ba7574746', NULL, '0', '0', NULL, '1', '0');

insert into AspNetUserRoles(UserId, RoleId)
values('319871ae-62fb-448b-bb9d-e7b4166310uf', 'fab4fac1-c546-41de-aebc-a14da6895711'),
('319871ae-62fb-448b-bb9d-e7b4166310uf', 'fab4fac1-c546-41de-aebc-a14da6895712'),
('319871ae-62fb-448b-bb9d-e7b4166310uf', 'fab4fac1-c546-41de-aebc-a14da6895713');
