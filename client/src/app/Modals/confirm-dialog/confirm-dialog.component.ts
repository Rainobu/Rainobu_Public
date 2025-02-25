import { Component, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.scss']
})
export class ConfirmDialogComponent implements OnInit {
  title :string;
  message :string;
  btnOkText:string;
  btnCancelText:string;
  result :boolean;
  constructor(public bsModalRf:BsModalRef) { }

  ngOnInit(): void {
  }
  confirm(){
    this.result = true;
    this.bsModalRf.hide();
  }

  decline(){
    this.result = false;
    this.bsModalRf.hide();
  }
}
