import { Component, Input, OnInit } from '@angular/core';
import { Rank } from 'src/app/_models/rank.model';
import { MembersService } from 'src/app/_services/members.service';
import { RankService } from 'src/app/_services/rank.service';

@Component({
  selector: 'app-label-tag',
  templateUrl: './label-tag.component.html',
  styleUrls: ['./label-tag.component.scss']
})
export class LabelTagComponent implements OnInit {
  @Input('imageUrl') imageUrl:string;
  @Input('knownAs') knownAs:string;
  @Input('pHeight') pHeight:number;
  @Input('point') point:number;
  @Input('title') title:string;
  allRank : Rank[] = [];
  bShadow :string="#ffffff";
  //setHeight :string= '40px';
  rank:string;
  border:number;
  tooltip:string;
  constructor(private rankService : RankService,
              private memberService:MembersService) {
     
   }
  ngAfterViewInit(): void {
    // console.log(this.imageUrl);
    // console.log(this.point);
    
  }
  ngOnInit(): void {
    //  this.setHeight = this.pHeight;
     this.memberService.getMemberByUserName(this.knownAs).subscribe(res => {
        this.imageUrl = res.photoUrl;
        this.point = res.point;
        this.title = res.title;
     })
     this.rankService.getAllRank().subscribe( (res:Rank[]) => {
        this.allRank = res;
        this.rank = this.CreateColor(this.point);
        this.tooltip =this.CreateRank(this.point);
     });
     this.border = this.pHeight * 0.05;
  }
  CreateRank(point:number){
    for (let i = 0; i < this.allRank.length ; i++) {
      if(point < this.allRank[i].maxPoint){
        // this.bShadow = this.allRank[i].color;
        return  this.allRank[i].name;
      }
      if(i == this.allRank.length-1){
        // this.bShadow = this.allRank[this.allRank.length-1].color;
        return this.allRank[this.allRank.length-1].name;
      }
    }
  }
  CreateColor(point:number){
    for (let i = 0; i < this.allRank.length ; i++) {
      if(point < this.allRank[i].maxPoint){
        // this.bShadow = this.allRank[i].color;
        // return  '0 0 0 2px '+ this.allRank[i].color;
        return  `0 0 0 ${this.border}px `+ this.allRank[i].color;
      }
      if(i == this.allRank.length-1){
        // this.bShadow = this.allRank[this.allRank.length-1].color;
        //return '0 0 0 2px '+ this.allRank[this.allRank.length-1].color;
        return `0 0 0 ${this.border}px `+ this.allRank[this.allRank.length-1].color;
      }
    }
  }
}
