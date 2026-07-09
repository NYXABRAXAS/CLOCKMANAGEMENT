# Admin Guide

## Granting the first Super Admin

There is no self-service way to become an admin (correctly - it shouldn't be possible via the
UI). Every account registers as `STANDARD_USER` by default. To promote the very first admin
account, run a one-time direct database update:

```sql
-- Find the user and the Super Admin role's IDs
SELECT Id FROM Users WHERE Email = 'you@example.com';
SELECT Id FROM Roles WHERE Code = 'SUPER_ADMIN';

-- Replace the user's role assignment
DELETE FROM UserRoles WHERE UserId = '<user-id>';
INSERT INTO UserRoles (Id, UserId, RoleId, CreatedAt, IsDeleted)
VALUES ('<new-guid>', '<user-id>', '<super-admin-role-id>', '<utc-now>', 0);
```

**This is a direct production database write - treat it with the same care as any other manual
database change against a live system.** After running it, the affected user must **log out and
back in** - JWT role/permission claims are only refreshed at login, so an existing session won't
pick up the new role until then. Once you have one Super Admin, every subsequent promotion should
go through the Admin Panel's own Users tab instead.

## Accessing the Admin Panel

Once logged in as a user holding any of `USERS:view`, `ROLES:view`, `AUDIT_LOGS:view`, or
`RELIGIONS:create`, an "Admin Panel" entry appears in the sidebar (hidden entirely for users
without any admin permission). It has four tabs, each independently permission-gated:

### Users

Search/paginated list of every account. Per user: activate/deactivate (you can't deactivate your
own account), unlock (resets failed-login count and any lockout), and reassign role via a dropdown.
Export the full list as CSV.

### Roles & Permissions

Pick a role from the dropdown, then toggle its `view`/`create`/`edit`/`delete` access per module in
the grid. **Super Admin's own permissions can't be edited** - attempting to returns a 409 - this is
a deliberate safety rail so the "Super Admin always has everything" guarantee the rest of the app
relies on can't be accidentally weakened through the UI itself. Admin can view but not edit the
`ROLES` module - only Super Admin can change what any role (including Admin) is allowed to do.

### Audit Log

Every admin action taken through the panel above (activate/deactivate, unlock, role reassignment,
permission grants/revokes) is logged here with the acting admin's email, a timestamp, and a
description. Export as CSV. (Some non-admin actions like login are also logged, since the
underlying audit service is available to any part of the app that chooses to call it - not every
action in the system is instrumented, only the admin-mutation surface this milestone added and a
handful of pre-existing auth events.)

### Religions

Create/edit/delete religion entries. Built-in religions (the 7 seeded ones plus "Other") can't be
deleted - they're referenced by seeded festival/quote data - only custom religions you add can be
removed. This is the mechanism the codebase's own design anticipated: adding a new religion never
needs a code change, just a new row here.
