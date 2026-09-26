import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';

@Component({standalone:true,imports:[FormsModule],templateUrl:'./doctor-dashboard.component.html'})
export class DoctorDashboardComponent implements OnInit{
  schedules:any[]=[];appointments:any[]=[];message='';error='';
  schedule={date:'',startTime:'09:00',endTime:'12:00',meetLink:'https://meet.google.com/'};
  rx:{[key:string]:{findings:string,medicines:string,advice:string}}={};
  constructor(private api:ApiService,public auth:AuthService){}
  ngOnInit(){this.load();}
  load(){
    this.api.get<any[]>('/schedules/mine').subscribe(x=>this.schedules=x);
    this.api.get<any[]>('/appointments/doctor').subscribe(x=>{
      this.appointments=x;
      x.forEach(a=>this.rx[a.id] ??= {findings:'',medicines:'',advice:''});
    });
  }
  create(){
    const payload={...this.schedule,startTime:this.withSeconds(this.schedule.startTime),endTime:this.withSeconds(this.schedule.endTime)};
    this.api.post<any>('/schedules',payload).subscribe({next:r=>{this.message=r.message;this.load()},error:e=>this.error=e.error?.message||'Could not create schedule.'});
  }
  publish(id:string){this.api.patch<any>(`/schedules/${id}`,{status:'Published'}).subscribe({next:r=>{this.message=r.message;this.load()},error:e=>this.error=e.error?.message||'Could not publish.'})}
  status(id:string,status:string){this.api.post<any>(`/appointments/${id}/status/${status}`).subscribe({next:r=>{this.message=r.message;this.load()},error:e=>this.error=e.error?.message||'Status update failed.'})}
  prescribe(id:string){const d=this.rx[id];if(!d)return;this.api.post<any>(`/prescriptions/appointment/${id}`,d).subscribe({next:r=>this.message=r.message,error:e=>this.error=e.error?.message||'Prescription failed.'})}
  private withSeconds(value:string){return value.length===5?`${value}:00`:value;}
}
