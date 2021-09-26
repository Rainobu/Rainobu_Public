import { Component, OnInit, Output, ViewChild } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';
import { PresenceService } from './_services/presence.service';
import { ViewportScroller } from '@angular/common';
import { Router, Scroll } from '@angular/router';
import { filter } from 'rxjs/operators';
import { MatSidenav } from '@angular/material/sidenav';
import { ThemeService } from './_services/theme.service';
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit{
  headlogo:string;
  bgtool:string;
  title = 'Rainobu';
  users: any;
  isDarkMode:boolean;
  constructor(private accountService:AccountService,
              private themeService: ThemeService,
              private presence:PresenceService,
              ){
                this.themeService.initTheme();
                this.isDarkMode = this.themeService.isDarkMode();
                console.log(this.isDarkMode)
                if(!this.isDarkMode){
                  this.headlogo ="./assets/images/logo.png"
                  this.bgtool = 'white';
                  }else{
                    this.headlogo ="./assets/images/logotransparent.png"
                    this.bgtool = 'black';
                  }
              }

  ngOnInit() {
    this.setCurrentUser(); 
  }
  toggleDarkMode() {
    this.isDarkMode = this.themeService.isDarkMode();

    this.isDarkMode
      ? this.themeService.update('light-mode')
      : this.themeService.update('dark-mode');
      if(this.isDarkMode){
      this.headlogo ="./assets/images/logo.png"
      this.bgtool = 'white';
      }else{
        this.headlogo ="./assets/images/logotransparent.png"
        this.bgtool = 'black';
      }
  }
  setCurrentUser(){
    const user:User = JSON.parse(localStorage.getItem('user'));
    if(user){
      this.accountService.setCurrentUser(user);
      this.presence.createHubConnection(user);
    }
    
  }

  

}
