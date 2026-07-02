import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ElementRef, Input, OnDestroy, ViewChild } from '@angular/core';

@Component({
  selector: 'app-sticky-buttons-container',
  imports: [CommonModule],
  templateUrl: './sticky-buttons-container.component.html',
  styleUrl: './sticky-buttons-container.component.scss',
})
export class StickyButtonsContainerComponent implements AfterViewInit, OnDestroy {
  @ViewChild('buttonsContainer') buttonsContainer!: ElementRef;

  @Input()
  fullWidth = false;

  fromModal = false;

  observer?: IntersectionObserver;

  ngAfterViewInit(): void {
    const modalContainer = this.buttonsContainer.nativeElement.closest('.dialog-with-sticky-buttons');
    this.fromModal = !!modalContainer;

    this.observer = new IntersectionObserver(
      ([e]) => {
        if (e.intersectionRatio < 1) {
          e.target.classList.add('stuck');
        } else {
          e.target.classList.remove('stuck');
        }
      },
      { threshold: [1], root: modalContainer }
    );

    this.observer.observe(this.buttonsContainer.nativeElement);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }
}
