# CassaComuneAnM – Features 🏖️💰

Welcome to the ultimate guide for **CassaComuneAnM 1.0.0** – the console app that keeps your common fund in check.  

---

## 1. Trips Management

- **Create Trip**  
  Set up a new trip with:
  - Name & code
  - Date
  - Coordinator & cashier
  - Country & currency (with exchange rate)
  - Participant list with personal budgets  

- **Retrieve Trip**  
  Pick a trip to manage from all saved trips.  

- **Delete Trip**  
  One wrong click and poof! Don’t worry – confirmation is required before deletion.  

---

## 2. Participants

- **Add Participant**  
  Add a new traveler with their personal budget.  

- **Remove Participant**  
  Remove a traveler, budget and deposits get recalculated.  

- **View Details**  
  See each participant’s personal budget, total deposits, and remaining balance.  

---

## 3. Expenses

- **Add Expense**  
  Record any group expense with:
  - Date
  - Description
  - Total amount
  - Tour Leader Free flag  

- **Custom Splitting**  
  - Expenses can be split among all participants or exclude some.  
  - App calculates pro-rata refunds for non-participants automatically.  
  - Tour Leader Free logic: coordinator doesn’t pay, but the cost is redistributed among paying participants.  

- **Show Expenses**  
  View all recorded expenses with date, description, amount, and beneficiaries.  

---

## 4. Deposits (aka “Money in the pot”)

- **Add Deposit**  
  Track how much each participant deposits into the common fund.  
  - Supports partial or full deposits.  
  - Validates deposit against personal budget.  
  - Offers to increase trip budget if someone wants to deposit more than their original budget.  

- **Show Deposits**  
  See all deposits with date, payer, and amount.  

- **Cash Summary**  
  - Participant-level: budget, deposited, and remaining amount.  
  - Trip-level: total budget, total deposited, total expenses, and cash balance.  

---

## 5. Console UX

- Auto-refreshing console: no endless scrolling!  
- Clear menus for trips, participants, expenses, and deposits.  
- Confirmations for deletions and critical actions.  

---

## 6. Business Logic Highlights

- **Tour Leader Free**: coordinator can attend for free, cost is redistributed.  
- **Automatic refunds**: non-participating participants get reimbursed correctly.  
- **Flexible budget**: budgets can be increased on the fly.  

---

## 7. Future Features

- Web app version for easier UI  
- Mobile app (Android) for on-the-go fund management  

---

> 💡 Tip: This app is serious about your money… but you can still have fun 😎
