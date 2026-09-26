import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';

@Component({standalone:true,imports:[FormsModule],templateUrl:'./admin-dashboard.component.html'})
export class AdminDashboardComponent implements OnInit{
  stats:any={};apps:any[]=[];users:any[]=[];message='';error='';roleFilter='';
  newAdmin={name:'',organizationEmail:'',personalEmail:'',role:'Admin'};
  constructor(private api:ApiService,public auth:AuthService){}
  ngOnInit(){this.load();}
  load(){this.api.get('/admin/dashboard').subscribe(x=>this.stats=x);this.api.get<any[]>('/admin/doctor-applications').subscribe(x=>this.apps=x);this.loadUsers();}
  loadUsers(){this.api.get<any[]>('/admin/users',this.roleFilter?{role:this.roleFilter}:undefined).subscribe(x=>this.users=x);}
  decision(id:string,d:string){this.api.post<any>(`/admin/doctors/${id}/decision/${d}`).subscribe({next:r=>{this.message=r.message;this.load()},error:e=>this.error=e.error?.message||'Action failed.'})}
  block(id:string,b:boolean){this.api.post<any>(`/admin/users/${id}/block/${b}`).subscribe({next:r=>{this.message=r.message;this.loadUsers()},error:e=>this.error=e.error?.message||'Action failed.'})}
  createAdmin(){this.api.post<any>('/admin/create-admin',this.newAdmin).subscribe({next:r=>{this.message=r.message;this.newAdmin={name:'',organizationEmail:'',personalEmail:'',role:'Admin'};this.loadUsers()},error:e=>this.error=e.error?.message||'Could not create admin.'})}
}
