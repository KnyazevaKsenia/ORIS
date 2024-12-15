const button= document.getElementById("button");

button.addEventListener('click', async () => {

    const latValue = document.getElementById("lat").value;
    const lonValue = document.getElementById("lon").value;
    const weatherP = document.getElementById("weather - info");

    let weatherResponse = await fetch(`http://localhost:5000/weather?${latValue}&${lonValue}`);
    if (weatherResponse.ok) {
        let weather = weatherResponse.json();
        weatherP.innerHTML = "City:";
    }
});

