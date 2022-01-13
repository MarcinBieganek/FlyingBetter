
//var flightTypeRadios = document.querySelectorAll('input[type=radio][name="FlightType"]');

function setFlightBackInputsReadonly(value) {
    document.getElementById("FlightBackFrom").readOnly = value;
    document.getElementById("FlightBackTo").readOnly = value;
    document.getElementById("FlightBackDate").readOnly = value;
    document.getElementById("FlightBackAdults").readOnly = value;
    document.getElementById("FlightBackChildren").readOnly = value;
}

function clearFlightBackInputs() {
    document.getElementById("FlightBackFrom").value = "NA";
    document.getElementById("FlightBackTo").value = "NA";
    document.getElementById("FlightBackDate").value = document.getElementById("Date").value;
    document.getElementById("FlightBackAdults").value = "1";
    document.getElementById("FlightBackChildren").value = "0";
}

document.getElementsByName("FlightType").forEach(flightTypeRadio =>
    flightTypeRadio.addEventListener('change', () => {
        if (flightTypeRadio.value == "OneWay") {
            setFlightBackInputsReadonly(true);
            clearFlightBackInputs();
            alert("Jedna");
        } else if (flightTypeRadio.value == "RoundTripStandard") {
            setFlightBackInputsReadonly(false);
            alert("dwie stand");
        } else {
            setFlightBackInputsReadonly(false);
            alert("dwie non standard");
        }
    })
);
