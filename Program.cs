using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var patients = new List<Patient>();
int nextId = 1;

app.MapGet("/", () => Results.Content(@"
<!DOCTYPE html>
<html>
<head>
<title>Hospital Management System</title>
<style>
body{font-family:Arial;background:#eef4f7;margin:0;text-align:center}
header{background:#087f8c;color:white;padding:25px}
main{background:white;margin:30px auto;padding:25px;max-width:750px;border-radius:12px}
input,button{padding:11px;margin:6px;border-radius:5px;border:1px solid #ccc}
button{background:#087f8c;color:white;cursor:pointer}
table{width:100%;margin-top:20px;border-collapse:collapse}
th,td{padding:10px;border:1px solid #ddd}
th{background:#087f8c;color:white}
</style>
</head>
<body>
<header><h1>Hospital Management System</h1>
<p>Patient Registration and Management Portal</p></header>
<main>
<h2>Register New Patient</h2>
<form id='patientForm'>
<input id='name' placeholder='Patient Name' required>
<input id='age' type='number' placeholder='Age' min='1' max='120' required>
<input id='disease' placeholder='Disease / Problem' required>
<input id='doctor' placeholder='Doctor Name' required>
<button type='submit'>Register Patient</button>
</form>
<h2>Patient Records</h2>
<button onclick='loadPatients()'>Refresh Records</button>
<table><thead><tr><th>ID</th><th>Name</th><th>Age</th><th>Problem</th><th>Doctor</th><th>Action</th></tr></thead>
<tbody id='records'></tbody></table>
<p id='message'></p>
</main>
<script>
async function loadPatients(){
 let r=await fetch('/api/patients');
 let data=await r.json();
 document.getElementById('records').innerHTML=data.map(p=>
 '<tr><td>'+p.id+'</td><td>'+p.name+'</td><td>'+p.age+'</td><td>'+p.disease+'</td><td>'+p.doctor+'</td><td><button onclick="deletePatient('+p.id+')">Delete</button></td></tr>'
 ).join('');
}
document.getElementById('patientForm').addEventListener('submit',async e=>{
 e.preventDefault();
 let patient={
 name:document.getElementById('name').value,
 age:Number(document.getElementById('age').value),
 disease:document.getElementById('disease').value,
 doctor:document.getElementById('doctor').value
 };
 let r=await fetch('/api/patients',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(patient)});
 let result=await r.json();
 document.getElementById('message').textContent=result.message;
 if(r.ok){e.target.reset();loadPatients();}
});
async function deletePatient(id){
 let r=await fetch('/api/patients/'+id,{method:'DELETE'});
 if(r.ok)loadPatients();
}
loadPatients();
</script>
</body>
</html>
", "text/html"));

app.MapGet("/api/patients", () => Results.Ok(patients));

app.MapPost("/api/patients", (PatientInput input) =>
{
    if (string.IsNullOrWhiteSpace(input.Name) ||
        string.IsNullOrWhiteSpace(input.Disease) ||
        string.IsNullOrWhiteSpace(input.Doctor) ||
        input.Age < 1 || input.Age > 120)
        return Results.BadRequest(new { message = "Please enter valid patient details." });

    var patient = new Patient
    {
        Id = nextId++,
        Name = input.Name.Trim(),
        Age = input.Age,
        Disease = input.Disease.Trim(),
        Doctor = input.Doctor.Trim()
    };

    patients.Add(patient);
    return Results.Ok(new { message = "Patient registered successfully!", patient });
});

app.MapDelete("/api/patients/{id:int}", (int id) =>
{
    var patient = patients.FirstOrDefault(p => p.Id == id);
    if (patient == null)
        return Results.NotFound(new { message = "Patient not found." });

    patients.Remove(patient);
    return Results.Ok(new { message = "Patient deleted successfully." });
});

app.Run();

public class Patient
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public string Disease { get; set; } = "";
    public string Doctor { get; set; } = "";
}

public class PatientInput
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public string Disease { get; set; } = "";
    public string Doctor { get; set; } = "";
}
