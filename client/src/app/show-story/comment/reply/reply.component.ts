import { Input, Component, OnInit, Output } from '@angular/core';
import { UntypedFormGroup, UntypedFormBuilder, Validators } from '@angular/forms';
import { ActivitiesType } from 'src/app/_models/activitiestype';
import { ActivitiesService } from 'src/app/_services/activities.service';
import { CommentService } from 'src/app/_services/comment.service';
// import { ReplyService } from '../../reply.service';

@Component({
  selector: 'app-reply-form',
  templateUrl: './reply.component.html',
  styleUrls: ['./reply.component.scss']
})
export class ReplyComponent implements OnInit {
  @Output('repCancel') repCancel: boolean;
  @Input('commentId') commentId: string;
  @Input('replyId') replyId: string;
  @Input('storyName') storyname:string;
  replyForm: UntypedFormGroup;
  activitiesType = ActivitiesType.writeComment;
  activitiesTimer = true;
  constructor(
    private fb: UntypedFormBuilder,
    public commentService:CommentService,
    private activitiesService:ActivitiesService
    ){}


  ngOnInit() {

    this.replyForm = this.fb.group({
      content: ['']
    });
  }

  get cId() {
    return this.commentId;
  }
  onReplyCancel() {
     // this.repCancel = false;

   // this.replyForm.reset();
  }

  onSubmit() {
    const submittedVal = {
      content: this.replyForm.value.content,
      comment: this.commentId,
      story_name:this.storyname
    }
    //console.log(submittedVal);
    this.commentService.sendComment(submittedVal.story_name,submittedVal.content,+submittedVal.comment,null).then(()=>{
      this.replyForm.reset();
      if(this.activitiesTimer){
        this.activitiesService.postActivities(this.activitiesType,submittedVal.story_name).subscribe(res =>{
          // console.log(res)
          this.activitiesTimer = false;
          setTimeout(() => {
            this.activitiesTimer = true;
          }, 300000);
        })
      }
  })
  }
}
