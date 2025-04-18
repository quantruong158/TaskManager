import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateStatusDialogComponent } from './create-status-dialog.component';

describe('CreateStatusDialogComponent', () => {
  let component: CreateStatusDialogComponent;
  let fixture: ComponentFixture<CreateStatusDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateStatusDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreateStatusDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
