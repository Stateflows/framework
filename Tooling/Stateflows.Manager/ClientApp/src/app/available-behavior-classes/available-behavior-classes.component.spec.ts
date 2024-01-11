import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AvailableBehaviorClassesComponent } from './available-behavior-classes.component';

describe('AvailableBehaviorClassesComponent', () => {
  let component: AvailableBehaviorClassesComponent;
  let fixture: ComponentFixture<AvailableBehaviorClassesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AvailableBehaviorClassesComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AvailableBehaviorClassesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
