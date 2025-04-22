import { Component, Input, OnInit } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { CommentService } from '../../core/services/comment.service';
import { CommentResponse } from '../../core/models/comment.model';

@Component({
  selector: 'comment-section',
  templateUrl: './comment-section.component.html',
  styleUrls: ['./comment-section.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
  ],
})
export class CommentSectionComponent implements OnInit {
  @Input() taskId!: number;
  comments: CommentResponse[] = [];
  contentControl: FormControl = new FormControl('', [Validators.required]);
  commentForm = new FormGroup({
    content: this.contentControl,
  });

  constructor(private commentService: CommentService) {}

  ngOnInit() {
    this.loadComments();
  }

  loadComments() {
    this.commentService.getComments(this.taskId).subscribe((comments) => {
      this.comments = comments;
    });
  }

  onSubmit() {
    if (this.commentForm.valid) {
      const comment = {
        taskId: this.taskId,
        content: this.commentForm.get('content')?.value || '',
      };

      this.commentService.createComment(comment).subscribe(() => {
        this.loadComments();
        this.commentForm.reset();
      });
    }
  }
}
