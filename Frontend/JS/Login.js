async function login(){

let email = document.getElementById("email").value.trim()
let password = document.getElementById("password").value.trim()

// ================= VALIDATION =================
// الإيميل
if(email === ""){
msg.innerText = "Email is required"
return
}

let emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

if(!emailPattern.test(email)){
msg.innerText = "Invalid email format"
return
}

// الباسورد
if(password === ""){
msg.innerText = "Password is required"
return
}

if(password.length < 6){
msg.innerText = "Password must be at least 6 characters"
return
} 

let response = await fetch("https://localhost:7209/api/auth/login",{

method:"POST",

headers:{
"Content-Type":"application/json"
},

body:JSON.stringify({

email:email,
password:password

})

})


let result = await response.text()

document.getElementById("msg").innerText = result

if(result === "Login Success")
{

window.location.href = "../HTML/index.html"

}

}