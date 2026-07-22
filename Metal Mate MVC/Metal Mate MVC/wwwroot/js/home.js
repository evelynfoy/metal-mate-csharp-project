// Updates the spot price information based on change of either the selected metal or currency.

document.addEventListener("DOMContentLoaded", () => {

    const selectedMetal = document.getElementById("SelectedMetal");
    const selectedCurrency = document.getElementById("SelectedCurrency");
    const spotPrice = document.getElementById("spotPrice");
    const exchangeRate = document.getElementById("exchangeRate");
    const currencySymbol = document.getElementById("currencySymbol");
    const updatedAt = document.getElementById("updatedAt");
    const errorMessage = document.getElementById("errorMessage");
    const updateButton = document.getElementById("updateButton");

    const url = selectedMetal.dataset.url;

    selectedMetal.addEventListener("change", loadSpotPrice);
    selectedCurrency.addEventListener("change", loadSpotPrice);
    updateButton.addEventListener("click", loadSpotPrice);

    async function loadSpotPrice() {

        try {
            const response = await fetch(
                `${url}?metal=${encodeURIComponent(selectedMetal.value)}&currency=${encodeURIComponent(selectedCurrency.value)}`
            );

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message);
            }

            const data = await response.json();

            spotPrice.textContent = data.price;
            exchangeRate.textContent = data.exchangeRate;
            currencySymbol.textContent = data.currencySymbol;
            updatedAt.textContent = data.updatedAt;

            hideError();
        }
        catch (error) {
            showError(error.message);
        }
    }

    function showError(message) {
        errorMessage.textContent = message;
        errorMessage.classList.remove("d-none");
    }

    function hideError() {
        errorMessage.textContent = "";
        errorMessage.classList.add("d-none");
    }

});