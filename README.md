# Metal Mate C# Pproject
Project to build a website to display metal prices and accept email alert requests which are polled hourly by an Azure function which sends out the email alerts.

## Description
The aim of this project is to build a website using an MVC C# project in Visual Studio that 
- Displays the current metal prices for a selected metal and currency using an API to retrieve the data. Price will be updated automatically on change of selections.
- The site will include authentication so the visitors can sign in and out.
- Once signed in the visitor will be able to set up email alert requests based on a metal price. This provides full CRUD functionality to a SQL server database to store these requests. Users will also be able to set a default metal and currency.
- There will be an Azure timer funtion backend that runs regurlaly and processes the email requests. If the current price falls within the parameters of the request an email will be sent out to the user.

  Example :-
  
  Sent me an alert if the gold price in EUR's is < 3,500.
  
  Send me an alert if the silver price in USD is > 3,000.
  
## Features
- Uses the Gold Price API at https://www.metals-api.com/ to retrieve the current metal prices. Error handling ensures that exceptions are sent on failure of the call and transient errors are retried 3 times before a final exception is returned.
- Displays 3 of the most common metal prices in EUR on the home page and then provides a drop down to select a metal and currency of choice to display the current price for this selection.
- Uses JavaScript to update the price details every minute, on change of the metal and currency selections,  and on clicking the Refresh Price button.

## Testing
### API Testing
- The call to the API is tested using XUnit tests to ensure it is functioning correctly.
- The tests include both Unit (with mocked responses) and Integration tests (with the real API).
- The tests include both positive and negative test cases and also that the retry logic is functioning correctly.

