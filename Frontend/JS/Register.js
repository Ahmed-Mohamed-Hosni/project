async function register(){
let name = document.getElementById("name").value.trim()
let email = document.getElementById("email").value.trim()
let password = document.getElementById("password").value.trim()

// ================= VALIDATION =================

if(name === ""){
msg.innerText = "Name is required"
return
}

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

let response = await fetch("https://localhost:7209/api/Auth/register",{

method:"POST",

headers:{
"Content-Type":"application/json"
},

body:JSON.stringify({
name:name,
email:email,
password:password
})

})

let result = await response.text()

document.getElementById("msg").innerText = result

if(result === "User Registered Successfully")
{

window.location.href = "../HTML/index.html"

}

}
