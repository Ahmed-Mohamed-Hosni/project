const startBtn = document.getElementById("startReport")
const reportSection = document.getElementById("reportSection")

startBtn.onclick = function(){

reportSection.scrollIntoView({
behavior:"smooth"
})

}



document.getElementById("submitReport").onclick = function(){

let problem = document.getElementById("problemType").value

if(problem === "")
{

alert("Please select problem type")

return

}

alert("Report Submitted Successfully")

}
document.getElementById("nav-btn logout").onclick=function() {
    window.location.href = "../HTML/Login.html"

}