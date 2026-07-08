export class CustomApiClient {
  protected customStringify(obj: unknown): string {
    return JSON.stringify(obj, (key, value) => {
      if (
        typeof value === 'string' &&
        /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$/.test(value)
      ) {
        value = new Date(value);
        const tzOffset = value.getTimezoneOffset();
        const sign = tzOffset > 0 ? '-' : '+';
        const hoursOffset = String(Math.abs(tzOffset / 60)).padStart(2, '0');
        const minutesOffset = String(Math.abs(tzOffset % 60)).padStart(2, '0');

        return (
          value.getFullYear() +
          '-' +
          String(value.getMonth() + 1).padStart(2, '0') +
          '-' +
          String(value.getDate()).padStart(2, '0') +
          'T' +
          String(value.getHours()).padStart(2, '0') +
          ':' +
          String(value.getMinutes()).padStart(2, '0') +
          ':' +
          String(value.getSeconds()).padStart(2, '0') +
          '.' +
          String(value.getMilliseconds()).padStart(3, '0') +
          sign +
          hoursOffset +
          ':' +
          minutesOffset
        );
      }

      return value;
    });
  }
}
