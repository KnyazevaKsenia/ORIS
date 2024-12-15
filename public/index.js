const button= document.getElementById("button");

button.addEventListener('click', async () => {

    const latValue = document.getElementById("lat").value;
    const lonValue = document.getElementById("lon").value;

    const weatherCity = document.getElementById("weather-city");
    const weatherDatetime = document.getElementById("weather-timezone");
    const weatherTemp = document.getElementById("weather-temp")

    let weatherResponse = await fetch(`http://localhost:5000/weather?${latValue}&${lonValue}`);
    if (weatherResponse.ok) {

        let weatherData = await weatherResponse.json();
        
        weatherDatetime.textContent+= weatherData.result.timezone;
        weatherTemp.textContent = "Temperature: " + weatherData.result.main.temp;
    }
});

