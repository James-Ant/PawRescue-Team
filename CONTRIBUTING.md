# 🐾 Paw Rescue — Member Contribution Tasks

## 1. Task assignments

| Member | Feature | Layers you'll touch |
|---|---|---|
| **Member 1** | Forgot password / reset password | Application service, mobile ViewModel |
| **Member 2** | Notifications screen (list, unread count, mark as read) | Repository (EF Core query), service, mobile ViewModel |
| **Member 3** | Map filters + marker clustering | Repository (EF Core filters), mobile service + ViewModel |
| **Member 4** | Admin user management + activity log | Service, repository, mobile ViewModel |

Write your name next to your number here so it is in the repo history:

- Member 1: ______________________
- Member 2: ______________________
- Member 3: ______________________
- Member 4: ______________________

---

### Member 1 — Forgot / Reset Password

**Files**
- `src/PawRescue.Application/Services/AuthService.cs` → `ForgotPasswordAsync`, `ResetPasswordAsync`
- `src/PawRescue.Mobile/ViewModels/AuthViewModels.cs` → `ForgotPasswordViewModel.SendCodeAsync`, `ResetAsync`

**Already done for you:** the API endpoints (`POST /api/auth/forgot-password`, `/reset-password`), the screen (`ForgotPasswordPage.xaml`), and `User.SetPasswordResetToken()` / `IsPasswordResetTokenValid()` in the Domain.

**Done when**
- [ ] In Swagger, `forgot-password` prints a 6-digit code in the API console
- [ ] `reset-password` with that code works, and you can log in with the new password
- [ ] A wrong or expired code returns a 400 error with a clear message
- [ ] In the app: Login → "Forgot password?" → enter email → enter code + new password → back on the login screen
- [ ] Unit test added (e.g. `User.IsPasswordResetTokenValid` returns false after 15 minutes / with a wrong hash)

---

### Member 2 — Notifications

**Files**
- `src/PawRescue.Infrastructure/Repositories/Repositories.cs` → `NotificationRepository` (3 methods)
- `src/PawRescue.Application/Services/NotificationService.cs` → `GetMyNotificationsAsync`, `GetUnreadCountAsync`, `MarkAsReadAsync`, `MarkAllAsReadAsync`
- `src/PawRescue.Mobile/ViewModels/DashboardViewModels.cs` → `NotificationsViewModel` (`LoadAsync`, `MarkAllReadAsync`, `OpenAsync`)

**Already done for you:** notifications are already *created* when a rescue is accepted or a status changes (`NotifyAsync`). You build the part that *shows* them. The screen is in `NotificationsPage.xaml`.

**Done when**
- [ ] Accept a report with a volunteer account → the reporter sees it in the Alerts tab
- [ ] Unread items have the orange dot; tapping one marks it read and opens the report
- [ ] "Mark all read" clears every dot
- [ ] The red badge on the Home bell shows the unread count
- [ ] A user can't mark someone else's notification (returns 403)
- [ ] Unit test added (e.g. `Notification.MarkAsRead` sets `IsRead` and `ReadAt`)

---

### Member 3 — Map Filters + Marker Clustering

**Files**
- `src/PawRescue.Infrastructure/Repositories/Repositories.cs` → `ReportRepository.GetInBoundingBoxAsync` and `SearchAsync` (the filter lines)
- `src/PawRescue.Mobile/Services/DeviceServices.cs` → `MarkerClusterer.Cluster`
- `src/PawRescue.Mobile/ViewModels/MapViewModel.cs` → `MinSeverity()`, `StatusFilter()`

**Already done for you:** the map, pins, pickers, search bar, and zooming in when a cluster pin is tapped.

**Done when**
- [ ] Choosing "High+" shows only High and Critical reports
- [ ] Choosing "Rescued" shows rescued reports; "Open" hides rescued and closed ones
- [ ] `GET /api/map/search?severity=Critical` in Swagger returns only Critical reports
- [ ] Zoomed out, nearby reports merge into one "N reports here" pin; zooming in splits them apart
- [ ] Unit test added (e.g. a severity-filter or distance test in `tests/PawRescue.Tests`)

---

### Member 4 — Admin User Management + Activity Log

**Files**
- `src/PawRescue.Application/Services/AdminService.cs` → `ChangeRoleAsync`, `SetActiveAsync`, `GetActivityAsync`
- `src/PawRescue.Infrastructure/Repositories/Repositories.cs` → `AuditLogRepository.GetRecentAsync`
- `src/PawRescue.Mobile/ViewModels/DashboardViewModels.cs` → `AdminDashboardViewModel.ManageUserAsync`

**Already done for you:** the Admin Dashboard screen (Overview, Users and Reports tabs), the API endpoints, and the `AuditLogs` table, which already receives entries.

**Done when**
- [ ] Log in as `admin@pawrescue.local` → Admin Dashboard → Users → tap a user → change their role
- [ ] Deactivating a user stops them from logging in; activating them lets them back in
- [ ] An admin cannot change their own role or deactivate themselves
- [ ] The Activity tab lists recent actions, newest first, with the user's name
- [ ] Unit test added (e.g. `User.Deactivate()` sets `IsActive = false`)

---

## 2. One-time setup

### Team leader (do this once)
1. Create a **private GitHub repository** named `paw-rescue`.
2. Unzip the project, then in the `PawRescue` folder:
   ```bash
   git init
   git add .
   git commit -m "Initial Paw Rescue project with team TODOs"
   git branch -M main
   git remote add origin https://github.com/<leader-username>/paw-rescue.git
   git push -u origin main
   ```
3. GitHub → **Settings → Collaborators** → invite all 4 members.
4. **Settings → Branches → Add rule** for `main`: tick **"Require a pull request before merging"** and **"Require approvals (1)"**. Now nobody can push straight to `main`, so every change is a reviewed PR with the author's name on it.
5. Create 4 **Issues** (one per member, copy the "Done when" checklist into each) and assign each to its owner.

### Every member
1. Accept the GitHub invite.
2. Set your identity. **Use the same email as your GitHub account**, or your commits won't count as yours:
   ```bash
   git config --global user.name "Your Full Name"
   git config --global user.email "your-github-email@example.com"
   ```
3. Clone and run the project once (see `README.md` → *Running it*):
   ```bash
   git clone https://github.com/<leader-username>/paw-rescue.git
   cd paw-rescue
   ```

---

## 3. Daily workflow (every member)

```bash
# 1. Start from the latest main
git checkout main
git pull

# 2. Create YOUR branch (once per feature)
git checkout -b feature/member2-notifications

# 3. Work in small steps; commit after each one
git add .
git commit -m "Notifications: implement GetForUserAsync paging query"

# 4. Push your branch
git push -u origin feature/member2-notifications
```

Branch names:
- `feature/member1-forgot-password`
- `feature/member2-notifications`
- `feature/member3-map-filters`
- `feature/member4-admin-users`

### Before you open a Pull Request
```bash
git checkout main && git pull
git checkout feature/member2-notifications
git merge main            # bring in teammates' merged work
dotnet build              # must build with 0 errors
dotnet test tests/PawRescue.Tests
```

### Pull Request (PR)
1. On GitHub: **Compare & pull request** → base `main` ← your branch.
2. Title: `Member 2 – Notifications screen`
3. In the description: what you built, how to test it, a **screenshot** of the working screen, and `Closes #<issue number>`.
4. Request a review from **one other member**. Suggested pairs: 1 ↔ 2, 3 ↔ 4.
5. The reviewer checks it out, runs it, and either approves or leaves comments. Fix the comments with new commits on the same branch.
6. After approval, click **Squash and merge**, or **Merge** if you want to keep every commit visible in history (recommended for grading).

---

## 4. Avoiding conflicts

Members 2, 3 and 4 all edit `Repositories.cs`, and Members 2 and 4 both edit `DashboardViewModels.cs`. Each person edits **different classes** inside those files, so Git usually merges them automatically. If you do get a conflict:

1. Run `git merge main` on your branch.
2. Open the file. Keep **both** sides between `<<<<<<<` and `>>>>>>>` when they are different methods.
3. `dotnet build`, then `git add .`, `git commit`, and push.

Don't reformat whole files (no "Format Document" on files you didn't change). That causes conflicts for everyone else.

---

## 5. How to show who did what (for the panel/adviser)
- **GitHub → Insights → Contributors**: commits and lines changed per member.
- **Pull requests tab**: each member's PR, reviews and comments.
- **Issues tab**: each feature assigned to and closed by its owner.
- `git log --author="Your Name" --oneline` lists your commits.
- Keep the filled-in names in section 1 of this file.

---

