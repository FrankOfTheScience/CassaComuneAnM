# CassaComuneAnM 🏖️💰

Managing a common fund for a trip has never been this… organized. 
## The problem
I am a coordinator for a Travel Agency and one of the tool the self-managed group use is the "Cassa comune" or "Common fund", in which the members of a travel group deposit a certain amount of cash to use for common expenses. Apparently, no app existed for it, yeah hard to believe it, I don't mean apps like "Tricount" or "Splitwise", those apps don't manage common fund expenses but _just_ common expenses with group compensation, so I built one.  

**CassaComuneAnM 1.0.0** is a console app to track trip participants, deposits, expenses, and even custom rules like “Tour Leader free” or per-person reimbursements.  

---

## Quick Features

- Create and manage trips with participants and personal budgets.  
- Record expenses and split them correctly among participants.  
- Track deposits with automatic balance calculation.  
- Special business logic for Tour Leaders and refunds.  
- Delete trips safely with confirmation.  
- Auto-refreshing console UI (no endless scrolling).  

---

## Future Plans

- Next major: Web app  
- Later: Android app (maybe skip straight to mobile)  

---

For a full breakdown of all features and how to use them, check out [FEATURES.md](./FEATURES.md).  

---

Tech: .NET 8 Console App + Spectre.Console + JSON storage  

💡 Pro tip: Don’t mess with the Tour Leader logic… it knows if you’re cheating 😉

## 📦 Download & Run

1. Go to the [GitHub Releases page](https://github.com/FrankOfTheScience/CassaComuneAnM/releases).  
2. Download `CassaComuneAnM_v1.0.0.zip`.  
3. Extract it anywhere on your PC.  
4. Double click on the file `CassaComuneAnM.exe`

That’s it! The app will guide you through creating trips, adding participants, and recording deposits/expenses.

