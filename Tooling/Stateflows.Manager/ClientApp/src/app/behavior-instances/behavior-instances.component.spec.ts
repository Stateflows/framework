import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BehaviorInstancesComponent } from './behavior-instances.component';

describe('BehaviorInstancesComponent', () => {
  let component: BehaviorInstancesComponent;
  let fixture: ComponentFixture<BehaviorInstancesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BehaviorInstancesComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(BehaviorInstancesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
