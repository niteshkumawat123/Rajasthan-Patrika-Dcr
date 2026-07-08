# Patrika Supply Alteration — User Manual

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [User Roles Overview](#2-user-roles-overview)
3. [Getting Started — Login & Account](#3-getting-started--login--account)
   - 3.1 [Login](#31-login)
   - 3.2 [First-Time Login — Change Password](#32-first-time-login--change-password)
   - 3.3 [Forgot Password](#33-forgot-password)
   - 3.4 [Logout](#34-logout)
4. [Executive / Sales Executive (SE) — Detailed Guide](#4-executive--sales-executive-se--detailed-guide)
   - 4.1 [Dashboard](#41-dashboard)
   - 4.2 [Creating a New Supply Change Request](#42-creating-a-new-supply-change-request)
   - 4.3 [Request History](#43-request-history)
   - 4.4 [Viewing Request Details (Audit Trail)](#44-viewing-request-details-audit-trail)
   - 4.5 [Profile Page](#45-profile-page)
5. [Zonal Head (ZH) — Detailed Guide](#5-zonal-head-zh--detailed-guide)
   - 5.1 [ZH Dashboard](#51-zh-dashboard)
   - 5.2 [Approving a Request](#52-approving-a-request)
   - 5.3 [Rejecting a Request](#53-rejecting-a-request)
   - 5.4 [Creating a New Request (ZH)](#54-creating-a-new-request-zh)
   - 5.5 [Filtering Requests](#55-filtering-requests)
   - 5.6 [ZH History](#56-zh-history)
6. [Head Office (HO) — Detailed Guide](#6-head-office-ho--detailed-guide)
   - 6.1 [HO Dashboard](#61-ho-dashboard)
   - 6.2 [Approving a Request (Push to ERP)](#62-approving-a-request-push-to-erp)
   - 6.3 [Rejecting a Request](#63-rejecting-a-request)
   - 6.4 [Filtering Requests](#64-filtering-requests)
   - 6.5 [HO History](#65-ho-history)
   - 6.6 [Branch Reports](#66-branch-reports)
7. [Complete Request Lifecycle — End-to-End Flow](#7-complete-request-lifecycle--end-to-end-flow)
8. [Request Statuses Explained](#8-request-statuses-explained)
9. [Notifications](#9-notifications)
10. [Frequently Asked Questions (FAQ)](#10-frequently-asked-questions-faq)

---

## 1. Introduction

**Patrika Supply Alteration** is a web-based application used by Rajasthan Patrika for managing newspaper supply change requests for agencies/agents. It allows field executives to request increases or decreases in newspaper supply, which then goes through a multi-level approval workflow — first by the **Zonal Head (ZH)** and then by the **Head Office (HO)**. Once fully approved at HO, the change is automatically pushed to the ERP system.

### Key Features

- Create supply increase/decrease requests for any agency
- Two-level approval workflow (Zonal Head ? Head Office)
- Automatic ERP integration on final approval
- Real-time notifications at each step
- Day-wise supply customization
- Complete audit trail for every request
- Branch-wise summary reports (HO)

---

## 2. User Roles Overview

| Role | Role ID(s) | Access | Key Capabilities |
|------|-----------|--------|------------------|
| **Executive / Sales Executive (SE)** | 1, 2, 3, 6 | Executive Dashboard | Create new requests, view own request history, track request status |
| **Zonal Head (ZH)** | 4 | ZH Dashboard + Executive Dashboard | All SE capabilities + Approve/Reject requests for their zone, forward approved requests to HO |
| **Head Office (HO)** | 7 | HO Dashboard | Final approval/rejection of requests, push approved changes to ERP, view branch reports |

> **Note:** A Zonal Head (Role 4) can also create new supply change requests, in addition to approving them.

---

## 3. Getting Started — Login & Account

### 3.1 Login

1. Open the application URL in your web browser.
2. You will see the **Login** page with the Patrika logo and the title "Patrika Supply Alteration".
3. Enter your **Employee ID** (Username) in the first field.
4. Enter your **Password** in the second field.
5. Click the **"Login ?"** button.

**What happens after login:**
- If you are a **Head Office (HO)** user (Role 7 only), you will be redirected to the **HO Dashboard**.
- If you are a **Zonal Head (ZH)** user (Role 4), you will be redirected to the **ZH Dashboard**.
- If you are an **Executive / SE** (Role 1, 2, 3, or 6), you will be redirected to the **Executive Dashboard**.

> **Important:** If you enter incorrect credentials, an error message will appear: *"Invalid credentials. Please check and try again."*

---

### 3.2 First-Time Login — Change Password

If this is your **first time logging in**, the system will automatically redirect you to the **Change Password** page.

1. You will see the Change Password form with your Employee Code pre-filled.
2. Enter your **New Password**.
3. Confirm the new password.
4. Click **"Change Password"**.
5. After successful password change, you will be automatically logged in and redirected to your dashboard.

> **Note:** You must change your password on first login. You cannot skip this step.

---

### 3.3 Forgot Password

If you forget your password:

1. On the Login page, click the **"Forgot Password?"** link.
2. Enter your **Employee ID** in the form.
3. Click **"Send Password ?"**.
4. Your password will be sent to your **registered email address**.
5. A success message will appear: *"Password has been sent to your registered email."*
6. Click **"? Back to Login"** to return to the Login page and log in with the received password.

> **Note:** If the Employee ID is not found, you will see the error: *"Employee ID not found."*

---

### 3.4 Logout

1. From any page, go to your **Profile** page (click the "Profile" icon in the bottom navigation bar).
2. Scroll down and click the **"?? Logout"** button.
3. You will be redirected back to the Login page.

---

## 4. Executive / Sales Executive (SE) — Detailed Guide

### 4.1 Dashboard

After logging in, you see the **Executive Dashboard** with:

#### Top Bar
- Your **name** and **role badge** are displayed at the top-right.

#### Cut-off Banner
- A banner shows the cut-off time: *"? Before Cut-off (5:00 PM) — Requests submitted now will process today."*
- Submit requests before 5:00 PM for same-day processing.

#### Statistics Cards (4 cards)
| Card | Color | Meaning |
|------|-------|---------|
| **Pending** | Orange | Number of your requests still awaiting approval |
| **Approved** | Green | Number of your requests that have been fully approved |
| **Today** | Blue | Number of requests you submitted today |
| **Rejected** | Red | Number of your requests that were rejected |

#### New Supply Change Request Button
- A prominent blue button **"? New Supply Change Request"** is displayed below the stats cards.
- Click this to create a new request (see Section 4.2).

#### Recent Requests
- Below the button, you see your **most recent requests** with:
  - Agency name and code
  - Drop point name
  - Creation date
  - Status pill (e.g., PENDING_ZH, HO_APPROVED, etc.)
  - Submitted by (creator name and code)
- Click **"View All"** to see complete history.
- Click any request to view its full details and audit trail.

#### Bottom Navigation Bar
| Icon | Label | Action |
|------|-------|--------|
| Home | Home | Go to Dashboard |
| New | New | Create a new request |
| Approve | Approve | *(Visible only if you also have ZH role)* — Go to ZH Dashboard |
| History | History | View all your past requests |
| Profile | Profile | View your profile and logout |

---

### 4.2 Creating a New Supply Change Request

This is the most important action for executives. Follow these steps carefully:

#### Step 1: Select Request Type
- At the top of the form, you see two chips: **"?? Increase"** and **"?? Decrease"**.
- **Increase** is selected by default.
- Click **"?? Decrease"** if you want to reduce supply for an agency.

#### Step 2: Search and Select Agency
1. In the **"Search Agency (Name or Code)"** field, start typing the agency name or agency code (minimum 2 characters).
2. A **dropdown list** will appear showing matching agencies from your assigned branches.
3. Each result shows:
   - **Agency Name** (bold)
   - **Agency Code / Drop Point Code / Branch Name / Drop Point Name**
4. Click on the desired agency to select it.
5. The following fields will be **auto-filled from ERP**:
   - Agency Name
   - Agency Code
   - Branch
   - Current Supply (from ERP)
   - Day-wise supply values (Mon–Sun)
6. A green checkmark **"? Agency found in ERP"** will appear.

> **Important:** Only agencies from your assigned branches will appear in search results.

#### Step 3: Set Effective Date
- The **Effective Date** field defaults to today's date.
- Change it if the supply change should take effect on a different date.

#### Step 4: Enter Supply Details
1. **Current Supply (ERP):** Auto-filled and read-only — shows the current supply from ERP.
2. **Increased/Decreased Quantity:** Enter the quantity you want to increase or decrease.
   - For **Increase**: Enter how many copies to add.
   - For **Decrease**: Enter how many copies to reduce.
   - The system will not allow decrease quantity to exceed current supply.
3. **Supply Preview:** A visual preview shows:
   - **Current** supply value
   - **Quantity** (increase/decrease amount)
   - **Total** (new supply after change)

#### Step 5 (Optional): Day-wise Supply
1. Check the **"Edit Day-wise Supply"** checkbox if you need to set different supply quantities for each day of the week.
2. When enabled, fields for **Monday through Sunday** will appear, pre-filled with current ERP values.
3. Modify individual day values as needed.
4. When day-wise is enabled, the overall quantity field becomes optional.

#### Step 6: Reason & Remarks
1. **Reason for Change (required):** Select a reason from the dropdown:
   - New residential colony opened
   - Subscriber base increased
   - Agency's area expanded
   - Festival / seasonal demand
   - Previous supply insufficient
   - Subscriber base decreased
   - Agency area reduced / split
   - Competition impact
   - Other
2. **Remarks for Approver (optional):** Add any additional notes for the Zonal Head.

#### Step 7: Submit
1. Click **"?? Submit for Approval"**.
2. If successful, you will see: *"Request submitted successfully! Redirecting..."*
3. You will be redirected to the Dashboard after 2 seconds.
4. The Zonal Head for your branch will receive a notification about the new request.

> **Important Rules:**
> - You **cannot submit a new request** for an agency that already has a **pending request**. You will see the message: *"A pending request already exists for this agency. Please wait for approval or cancellation before submitting a new one."*
> - You must select an agency before submitting.
> - Quantity must be greater than 0 (unless day-wise is enabled).

---

### 4.3 Request History

1. From the Dashboard, click **"View All"** or the **"History"** icon in the bottom navigation.
2. You see a list of **all your submitted requests** with:
   - Agency name
   - Branch code
   - Creation date
   - Supply change (Current ? New, with ? or ? indicator)
   - Agency code
   - Creator name
   - Status pill
3. Use the **search bar** at the top to filter by agency name, code, or status.
4. Click any request to view its **full details and audit trail**.

---

### 4.4 Viewing Request Details (Audit Trail)

When you click on any request (from Dashboard or History), a **detail modal** opens showing:

1. **Status badge** and **submission date/time**
2. **Supply change summary:**
   - Current supply ? New supply ? Difference (with color coding: green for increase, red for decrease)
3. **Request details:**
   - Agency name and code
   - Branch name and code
   - Effective date
   - Submitted by (name and code)
4. **Reason & Remarks:**
   - Reason for change
   - Executive's remarks
5. **Approval trail:**
   - ZH Remarks (if ZH has taken action)
   - HO Remarks (if HO has taken action)

---

### 4.5 Profile Page

1. Click the **"Profile"** icon in the bottom navigation bar.
2. View your account information:
   - Employee ID
   - Name
   - Branch
   - Department
   - Zone
   - Reporting To (your manager's name)
   - Status (Active/Inactive)
3. Click **"?? Logout"** to log out.

---

## 5. Zonal Head (ZH) — Detailed Guide

### 5.1 ZH Dashboard

After logging in as a Zonal Head, you see the **ZH Dashboard** with:

#### Top Bar
- Your **name** and **"Zonal Head"** role badge.

#### Statistics Cards (4 interactive cards)
| Card | Color | Meaning | Click Action |
|------|-------|---------|--------------|
| **Awaiting Me** | Orange | Requests waiting for your approval | Shows pending requests |
| **I Approved** | Green | Requests you have approved | Shows your approved requests |
| **At HO** | Blue | Requests forwarded to HO (pending HO action) | Shows requests at HO |
| **Rejected** | Red | Requests you have rejected | Shows rejected requests |

> **Tip:** Click any stats card to filter the request list below it.

#### New Supply Change Request Button
- If you also have an executive role, you will see the **"? New Supply Change Request"** button.

#### Pending Requests List
- Shows requests **pending your approval** by default.
- Each request card shows:
  - ??/?? icon (Increase/Decrease)
  - Agency name
  - Publication / Edition
  - Branch code
  - Creation date
  - Supply change (Current ? New with ?/?)
  - Created by
  - Status: **PENDING_ZH**
  - Quick action buttons: **? Approve** and **? Reject**

#### Bottom Navigation Bar
| Icon | Label | Action |
|------|-------|--------|
| Home | Home | Go to ZH Dashboard |
| New | New | Create a new request *(if ZH has executive role)* |
| History | History | View your submitted requests *(if ZH has executive role)* |
| Profile | Profile | View profile and logout |

---

### 5.2 Approving a Request

You can approve a request in two ways:

#### Method 1: Quick Approve from List
1. On the Dashboard, find the pending request.
2. Click the **?** (green checkmark) button on the right side of the request card.
3. A **remarks modal** will open with title **"Approve & Forward to HO"**.
4. Enter **Remarks** (optional).
5. Click **"Forward to HO ?"**.
6. The request status changes from **PENDING_ZH** to **PENDING_HO**.
7. A success message appears: *"Request approved."*
8. The page reloads after 1 second.

#### Method 2: Approve from Detail View
1. Click on the request card to open the **detail modal**.
2. Review all request details (agency, supply change, reason, remarks).
3. Click the **"?? Approve"** button at the bottom of the modal.
4. Enter remarks in the remarks modal and confirm.

**What happens after ZH Approval:**
- The request status changes to **PENDING_HO**.
- HO users receive a notification: *"Request #[ID] has been approved by ZH ([Your Name]) and forwarded to HO for final approval."*
- The request creator (Executive) receives a notification: *"Your request #[ID] has been approved by ZH and forwarded to HO for final approval."*

---

### 5.3 Rejecting a Request

#### Method 1: Quick Reject from List
1. Click the **?** (red cross) button on the request card.
2. A **remarks modal** opens with title **"Reject Request"**.
3. Enter **Remarks (required)** — you must provide a reason for rejection.
4. Click **"Reject"**.
5. The request status changes to **REJECTED** (ZH level).

#### Method 2: Reject from Detail View
1. Click on the request card to open the detail modal.
2. Click the **"? Reject"** button at the bottom.
3. Enter rejection reason and confirm.

**What happens after ZH Rejection:**
- The request creator receives a notification: *"Your request #[ID] has been rejected by ZH ([Your Name]). Remarks: [Your Remarks]"*

> **Important:** Rejection remarks are **mandatory**. You must enter a reason before rejecting.

---

### 5.4 Creating a New Request (ZH)

If you have an executive role (Role 1, 3, 4, or 6), you can also create new supply change requests:

1. Click **"? New Supply Change Request"** on the ZH Dashboard, or click the **"New"** icon in the bottom navigation.
2. The process is **exactly the same** as described in Section 4.2.
3. Your request will also follow the same approval flow (it will be assigned to the ZH of the relevant branch).

---

### 5.5 Filtering Requests

Click any of the **4 stats cards** on the ZH Dashboard to filter the request list:

| Filter | Shows |
|--------|-------|
| **Awaiting Me** | Requests pending your approval (PENDING_ZH) |
| **I Approved** | Requests you have approved (forwarded to HO or fully approved) |
| **At HO** | Requests that are currently pending at HO (PENDING_HO) |
| **Rejected** | Requests you have rejected |

The active filter card is highlighted with a blue outline. The section title and count update accordingly.

---

### 5.6 ZH History

1. Click the **"History"** icon in the bottom navigation.
2. View all requests **submitted by you** (as an executive).
3. This page is the same as the Executive History page (Section 4.3).

---

## 6. Head Office (HO) — Detailed Guide

### 6.1 HO Dashboard

After logging in as an HO user, you see the **HO Dashboard** with:

#### Top Bar
- Your **name** and **"Jaipur HO"** role badge.

#### Statistics Cards (5 interactive cards)
| Card | Color | Meaning | Click Action |
|------|-------|---------|--------------|
| **Awaiting** | Orange | Requests pending HO approval | Shows pending requests |
| **Approved** | Green | Requests approved by you | Shows your approved requests |
| **Increased** | Blue | Total increase requests (approved) | Shows approved increase requests |
| **Decreased** | Red | Total decrease requests (approved) | Shows approved decrease requests |
| **Rejected** | Border only | Requests rejected by you | Shows your rejected requests |

> **Tip:** Click any card to filter the request list.

#### Pending Requests List
- Shows requests **pending HO approval** by default.
- Each request card shows:
  - ??/?? icon
  - Agency name and drop point name
  - Agency code
  - Publication / Edition
  - Branch code and name
  - Creation date
  - Supply change (Current ? New with ?/?)
  - Created by
  - ZH approver name and ZH remarks (if available)
  - Status: **PENDING_HO**
  - Quick action buttons: **? Approve** and **? Reject**

#### Bottom Navigation Bar
| Icon | Label | Action |
|------|-------|--------|
| Home | Home | Go to HO Dashboard |
| History | History | View all request history |
| Reports | Reports | View branch-wise summary reports |
| Profile | Profile | View profile and logout |

---

### 6.2 Approving a Request (Push to ERP)

This is the **final approval step**. When HO approves a request, the supply change is **automatically pushed to the ERP system**.

#### Method 1: Quick Approve from List
1. On the Dashboard, find the pending request.
2. Click the **?** button on the right side.
3. A **remarks modal** opens with title **"Approve & Push to ERP"**.
4. Enter **Remarks** (optional).
5. Click **"Approve & Push ERP"**.
6. The supply change is pushed to ERP.
7. Success message: *"Request approved and pushed to ERP."*

#### Method 2: Approve from Detail View
1. Click on the request card to open the detail modal.
2. Review all details including ZH remarks.
3. Close the modal and use the quick approve button, or approve directly.

**What happens after HO Approval:**
- The request status changes to **HO_APPROVED**.
- The supply change is **pushed to ERP** automatically.
- The Zonal Head of the relevant branch receives a notification: *"Request #[ID] has been approved by HO ([Your Name]) and pushed to ERP."*
- The request creator (Executive) receives a notification: *"Your request #[ID] has been approved by HO and pushed to ERP."*

---

### 6.3 Rejecting a Request

#### Method 1: Quick Reject from List
1. Click the **?** button on the request card.
2. A **remarks modal** opens with title **"Reject Request"**.
3. Enter **Reason for rejection**.
4. Click **"Reject"**.

**What happens after HO Rejection:**
- The request status changes to **HO_REJECTED**.
- The Zonal Head receives a notification: *"Request #[ID] has been rejected by HO ([Your Name]). Remarks: [Your Remarks]"*
- The request creator receives a notification: *"Your request #[ID] has been rejected by HO. Remarks: [Your Remarks]"*

---

### 6.4 Filtering Requests

Click any of the **5 stats cards** on the HO Dashboard:

| Filter | Shows |
|--------|-------|
| **Awaiting** | Requests pending HO approval (PENDING_HO) |
| **Approved** | Requests approved by you (HO_APPROVED) |
| **Increased** | All approved increase requests |
| **Decreased** | All approved decrease requests |
| **Rejected** | Requests rejected by you (HO_REJECTED) |

---

### 6.5 HO History

1. Click the **"History"** icon in the bottom navigation.
2. View **all requests** across all branches (that you have access to).
3. Search and filter by agency name, code, or status.
4. Click any request to view full details.

---

### 6.6 Branch Reports

This feature is exclusive to HO users.

1. Click the **"Reports"** icon in the bottom navigation.
2. You see a **Branch-wise Summary Report** page.
3. **Select a date** using the date picker at the top (defaults to today).
4. For each branch, the report shows:

   **Today's Summary:**
   - Total requests for the day
   - ? Increase: Total increased copies and number of increase entries
   - ? Decrease: Total decreased copies and number of decrease entries

   **Comparison with Yesterday:**
   - Increase difference vs yesterday
   - Decrease difference vs yesterday
   - Total requests difference vs yesterday

> **Tip:** Change the date to view historical branch summaries.

---

## 7. Complete Request Lifecycle — End-to-End Flow

Below is the complete journey of a supply change request from creation to ERP push:

```
???????????????????????????????????????????????????????????????????????
?                        REQUEST LIFECYCLE                            ?
???????????????????????????????????????????????????????????????????????
?                                                                     ?
?  STEP 1: Executive Creates Request                                  ?
?  ?? Selects Increase or Decrease                                    ?
?  ?? Searches and selects an Agency                                  ?
?  ?? Enters quantity and reason                                      ?
?  ?? Optionally sets day-wise supply                                 ?
?  ?? Submits ? Status: PENDING_ZH                                   ?
?       ?                                                             ?
?       ? (Notification sent to Zonal Head)                           ?
?                                                                     ?
?  STEP 2: Zonal Head Reviews                                        ?
?  ?? Sees request on ZH Dashboard under "Awaiting Me"                ?
?  ?? Reviews details, supply change, and reason                      ?
?  ?? Takes action:                                                   ?
?       ?                                                             ?
?       ?? ? APPROVE ? Status: PENDING_HO                            ?
?       ?    ?  (Notification sent to HO and Executive)               ?
?       ?    ?                                                        ?
?       ?                                                             ?
?       ?  STEP 3: Head Office Reviews                                ?
?       ?  ?? Sees request on HO Dashboard under "Awaiting"           ?
?       ?  ?? Reviews details including ZH remarks                    ?
?       ?  ?? Takes action:                                           ?
?       ?       ?                                                     ?
?       ?       ?? ? APPROVE ? Status: HO_APPROVED                   ?
?       ?       ?    ?? Supply change PUSHED TO ERP ?                 ?
?       ?       ?       (Notification to ZH and Executive)            ?
?       ?       ?                                                     ?
?       ?       ?? ? REJECT ? Status: HO_REJECTED                    ?
?       ?            (Notification to ZH and Executive)               ?
?       ?                                                             ?
?       ?? ? REJECT ? Status: REJECTED (ZH level)                    ?
?            (Notification sent to Executive)                         ?
?                                                                     ?
???????????????????????????????????????????????????????????????????????
```

### Summary Table

| Step | Actor | Action | Resulting Status |
|------|-------|--------|-----------------|
| 1 | Executive/SE | Creates and submits request | **PENDING_ZH** |
| 2a | Zonal Head | Approves request | **PENDING_HO** |
| 2b | Zonal Head | Rejects request | **REJECTED** |
| 3a | Head Office | Approves request | **HO_APPROVED** (pushed to ERP) |
| 3b | Head Office | Rejects request | **HO_REJECTED** |

---

## 8. Request Statuses Explained

| Status | Meaning | Who Can See | Next Action |
|--------|---------|------------|-------------|
| **PENDING_ZH** | Request submitted, waiting for Zonal Head approval | Executive (creator), ZH | ZH approves or rejects |
| **PENDING_HO** | Approved by ZH, waiting for Head Office approval | Executive, ZH, HO | HO approves or rejects |
| **HO_APPROVED** | Fully approved by HO and pushed to ERP | All | No further action — completed ? |
| **HO_REJECTED** | Rejected by Head Office | Executive, ZH, HO | Executive may create a new request |
| **REJECTED** | Rejected by Zonal Head | Executive, ZH | Executive may create a new request |

---

## 9. Notifications

The system automatically sends notifications at key points in the workflow:

| Event | Who is Notified | Message |
|-------|----------------|---------|
| New request submitted | Zonal Head (of the branch) | "A new supply alteration request has been submitted by [Name] ([EmpCode]) for agent [AgentCode]. Please review and take action." |
| ZH approves request | All HO users + Request creator | "Request #[ID] has been approved by ZH and forwarded to HO for final approval." |
| ZH rejects request | Request creator | "Your request #[ID] has been rejected by ZH ([Name]). Remarks: [Remarks]" |
| HO approves request | ZH (of the branch) + Request creator | "Request #[ID] has been approved by HO and pushed to ERP." |
| HO rejects request | ZH (of the branch) + Request creator | "Your request #[ID] has been rejected by HO. Remarks: [Remarks]" |

---

## 10. Frequently Asked Questions (FAQ)

### Q1: I submitted a request but want to change it. Can I edit it?
**A:** No. Once a request is submitted, it cannot be edited. You must wait for the current request to be approved or rejected. If rejected, you can create a new request with the correct details.

### Q2: I get the error "A pending request already exists for this agency." What should I do?
**A:** There is already an active (pending) request for the same agency and drop point. Wait for the existing request to be approved or rejected before submitting a new one.

### Q3: Can I submit a decrease request for more copies than the current supply?
**A:** No. The system will show an error: *"Decrease quantity cannot exceed current supply."* You can only decrease up to the current supply amount.

### Q4: What is "Day-wise Supply"?
**A:** Day-wise supply allows you to set different supply quantities for each day of the week (Monday through Sunday). This is useful when an agency needs different quantities on different days. Check the "Edit Day-wise Supply" checkbox to enable this feature.

### Q5: What happens when HO approves my request?
**A:** When HO approves, the supply change is automatically pushed to the ERP system. The new supply quantity will take effect from the effective date you specified in the request.

### Q6: I forgot my password. What should I do?
**A:** On the Login page, click "Forgot Password?" and enter your Employee ID. Your password will be emailed to your registered email address.

### Q7: Who can I contact if the system is not working?
**A:** Contact your IT department or system administrator for technical support.

### Q8: Why can't I see the "New" button on my dashboard?
**A:** The "New Supply Change Request" button is only visible to users with executive roles (Role 1, 2, 3, 4, or 6). If you are an HO-only user (Role 7), you can only approve/reject requests, not create them.

### Q9: Can a Zonal Head create supply change requests?
**A:** Yes. If the Zonal Head also has an executive role, they can create requests from the ZH Dashboard using the "New Supply Change Request" button or the "New" tab in the bottom navigation.

### Q10: How do I know the status of my request?
**A:** Go to your Dashboard and check the "Recent Requests" section, or go to "History" to see all your requests with their current statuses.

---

*Document Version: 1.0*
*Application: Patrika Supply Alteration*
*Last Updated: June 2025*
