import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';

@Component({standalone:true,imports:[DatePipe,RouterLink],templateUrl:'./patient-dashboard.component.html'})
export class PatientDashboardComponent implements OnInit{
  items:any[]=[];message='';error='';
  constructor(private api:ApiService,public auth:AuthService){}
  ngOnInit(){this.load();}
  load(){this.api.get<any[]>('/appointments/mine').subscribe(x=>this.items=x);}
  cancel(id:string){this.api.post<any>(`/appointments/${id}/cancel`).subscribe({next:r=>{this.message=r.message;this.load()},error:e=>this.error=e.error?.message||'Could not cancel.'})}
}
