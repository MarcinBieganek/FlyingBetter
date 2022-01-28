
function setFlightBackInputsReadonly(value, dateValue, directValue) {
    document.getElementById("FlightBackFrom").readOnly = value;
    document.getElementById("FlightBackTo").readOnly = value;
    document.getElementById("FlightBackDate").readOnly = dateValue;
    document.getElementById("FlightBackAdults").readOnly = value;
    document.getElementById("FlightBackChildren").readOnly = value;
    document.getElementById("FlightBackDirect").disabled = directValue;
}

function clearFlightBackInputs() {
    document.getElementById("FlightBackFrom").value = "NA";
    document.getElementById("FlightBackTo").value = "NA";
    document.getElementById("FlightBackDate").value = document.getElementById("Date").value;
    document.getElementById("FlightBackAdults").value = "1";
    document.getElementById("FlightBackChildren").value = "0";
}

function moveValuesToFlightBackInputs() {
    document.getElementById("FlightBackFrom").value = document.getElementById("To").value;
    document.getElementById("FlightBackTo").value = document.getElementById("From").value;
    document.getElementById("FlightBackAdults").value = document.getElementById("Adults").value;
    document.getElementById("FlightBackChildren").value = document.getElementById("Children").value;
}

document.addEventListener('DOMcontentLoaded', () => {
    if (document.querySelector('input[id="FlightType"]:checked').value == "OneWay") {
        setFlightBackInputsReadonly(true, true, true);
        clearFlightBackInputs();
    } else if (document.querySelector('input[id="FlightType"]:checked').value == "RoundTripStandard") {
        setFlightBackInputsReadonly(true, false, false);
        moveValuesToFlightBackInputs();
    } else {
        setFlightBackInputsReadonly(false, false, false);
        moveValuesToFlightBackInputs();
    }
});

// make changes in form when flight type is selected
document.getElementsByName("FlightType").forEach(flightTypeRadio =>
    flightTypeRadio.addEventListener('change', () => {
        if (flightTypeRadio.value == "OneWay") {
            setFlightBackInputsReadonly(true, true, true);
            clearFlightBackInputs();
        } else if (flightTypeRadio.value == "RoundTripStandard") {
            setFlightBackInputsReadonly(true, false, false);
            moveValuesToFlightBackInputs();
        } else {
            setFlightBackInputsReadonly(false, false, false);
            moveValuesToFlightBackInputs();
        }
    })
);

// When round trip standard auto update form input value for flight back
document.getElementById("From").addEventListener('input', () => {
    if (document.querySelector('input[id="FlightType"]:checked').value == "RoundTripStandard") {
        document.getElementById("FlightBackTo").value = document.getElementById("From").value;
    }
});

document.getElementById("To").addEventListener('input', () => {
    if (document.querySelector('input[id="FlightType"]:checked').value == "RoundTripStandard") {
        document.getElementById("FlightBackFrom").value = document.getElementById("To").value;
    }
});

document.getElementById("Adults").addEventListener('input', () => {
    if (document.querySelector('input[id="FlightType"]:checked').value == "RoundTripStandard") {
        document.getElementById("FlightBackAdults").value = document.getElementById("Adults").value;
    }
});

document.getElementById("Children").addEventListener('input', () => {
    if (document.querySelector('input[id="FlightType"]:checked').value == "RoundTripStandard") {
        document.getElementById("FlightBackChildren").value = document.getElementById("Children").value;
    }
});