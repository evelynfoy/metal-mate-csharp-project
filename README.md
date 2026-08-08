# Metal Mate C# Project
Project to build a website to display metal prices and accept email alert requests which are polled hourly by an Azure function which sends out the email alerts.

## Description
The aim of this project is to build a website using an MVC C# project in Visual Studio that 
- Displays the current metal prices for a selected metal and currency using an API to retrieve the data. Price will be updated automatically on change of selections.
- The site will include authentication so the visitors can sign in and out.
- Once signed in the visitor will be able to access a user profile page and set a favourite metal and currency that will then be defaulted to the drop downs lists on the home page.
- Once signed in the visitor will also be able to set up email alert requests based on a metal price. This provides full CRUD functionality to a SQL server database to store these requests. 
- There will be an Azure timer funtion backend that runs regurlaly and processes the email requests. If the current price falls within the parameters of the request an email will be sent out to the user.

  Example :-
  
  Sent me an alert if the gold price in EUR's is < 3,500.
  
  Send me an alert if the silver price in USD is > 3,000.
  
## Features
- Uses the Gold Price API at https://www.metals-api.com/ to retrieve the current metal prices. Error handling ensures that exceptions are sent on failure of the call and transient errors are retried 3 times before a final exception is returned.
- Displays 3 of the most common metal prices in EUR on the home page and then provides a drop down to select a metal and currency of choice to display the current price for this selection.
- Uses JavaScript to update the price details every minute, on change of the metal and currency selections,  and on clicking the Refresh Price button.
- Uses Microsoft.AspNetCore.Identity tables to hold user specific profile information such as favourite metal and currency.

## Testing
### API Testing
- The call to the API is tested using XUnit tests to ensure it is functioning correctly.
- The tests include both Unit (with mocked responses) and Integration tests (with the real API).
- The tests include both positive and negative test cases and also that the retry logic is functioning correctly.
### User Authenticating Testing
- The retrieval and writing of user data to the Microsoft.AspNetCore.Identity AspNetUsers table is tested in both the unit tests and integration tests for the User Profile page and the home page which reads this information.
- The unit tests use a Moq mock to setup a mock user manager which returns either a valid user entry or an exception.
- The integration tests use a CustomWebApplicationFactory to setup a user manager with a scope that contains a connection to an in-memory Sqlite database that it uses for the tests.
- The CustomWebApplicationFactory also sets up a mock API service to return a valid response for the metal price API call and a list of metals for the dropdown list.
