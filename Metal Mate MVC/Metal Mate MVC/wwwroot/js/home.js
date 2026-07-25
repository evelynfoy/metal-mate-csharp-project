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
    const goldSpotPrice = document.getElementById("goldSpotPrice");
    const silverSpotPrice = document.getElementById("silverSpotPrice");
    const platinumSpotPrice = document.getElementById("platinumSpotPrice");



    const url = selectedMetal.dataset.url;

    selectedMetal.addEventListener("change", loadSpotPrice);
    selectedCurrency.addEventListener("change", loadSpotPrice);
    updateButton.addEventListener("click", loadSpotPrice);

    // Refresh every minute
    setInterval(loadSpotPrice, 60000);

    let loading = false;

    async function loadSpotPrice() {

        if (loading) {
            return;
        }

        loading = true;
        updateButton.disabled = true;

        try {
            // Get price for current selections
            let response = await fetch(
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

            // Get current Gold Euro Price
            response = await fetch(
                `${url}?metal=${encodeURIComponent("XAU")}&currency=${encodeURIComponent("EUR")}`);

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message);
            }

            const goldData = await response.json();
            goldSpotPrice.textContent = "€" + goldData.price;

            // Get current Silver Euro Price
            response = await fetch(
                `${url}?metal=${encodeURIComponent("XAG")}&currency=${encodeURIComponent("EUR")}`);

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message);
            }

            const silverData = await response.json();
            silverSpotPrice.textContent = "€" + silverData.price

            // Get Platinum Euro Price
            response = await fetch(
                `${url}?metal=${encodeURIComponent("XPT")}&currency=${encodeURIComponent("EUR")}`);

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message);
            }

            const platinumData = await response.json();
            platinumSpotPrice.textContent = "€" + platinumData.price

            hideError();
        }
        catch (error) {
            showError(error.message);
        }
        finally {
            loading = false;
            updateButton.disabled = false;
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