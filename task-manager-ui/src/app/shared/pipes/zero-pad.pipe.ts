import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'zeroPad',
  standalone: true,
})
export class ZeroPadPipe implements PipeTransform {
  transform(value: number): string {
    return value.toString().padStart(4, '0');
  }
}
