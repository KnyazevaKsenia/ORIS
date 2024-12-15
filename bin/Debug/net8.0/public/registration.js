const getUserInfo = () => {
    let login = document.getElementById("email").value;
    let password = document.getElementById("password").value;
    let approvePassword = document.getElementById("repeat-password").value;

    if (password == approvePassword) {
        return { login, password };
    } else {
        console.log("Passwords do not match");
        return null;
    }
}

const sendData = async (data) => {
    if (!data) {
        alert("Passwords do not match. Please try again.");
        return;
    }

    try {
        const response = await fetch("http://localhost:5000/registration/saveUser", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(data),
        });

        if (!response.ok) {
            throw new Error('Network response was not ok ' + response.statusText);
        }

        const responseData = await response.json();
        alert(responseData);
    } catch (error) {
        console.error('Error:', error);
        alert('Registration failed');
    }
}

let sendButton = document.getElementById("sendButton");
sendButton.addEventListener('click', () => sendData(getUserInfo()));






