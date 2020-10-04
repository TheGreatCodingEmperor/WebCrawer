import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { saveAs } from 'file-saver';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {
  // public forecasts: WeatherForecast[];
  public stocks :Stock[]=[];
  baseUrl = '';
  cols = [];

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
    // this.http.get<WeatherForecast[]>(baseUrl + 'weatherforecast').subscribe(result => {
    //   this.forecasts = result;
    // }, error => console.error(error));
    this.http.get<any>(baseUrl + 'weatherforecast/stock').subscribe(result => {
      this.stocks = result;
      this.cols = Object.keys(result.result[0]);
    }, error => console.error(error));
  }

  download() {
    this.http.get<Blob>(this.baseUrl + 'weatherforecast/crawler', { headers: { responseType: 'blob' } }).subscribe((result: any) => {
      // const blob = new Blob([result], { type: 'text/csv' });
      // const url = window.URL.createObjectURL(blob);
      // window.open(url);
      result.fileContents = this.b64DecodeUnicode(result.fileContents);
      var a = document.createElement('a');
      var file = new Blob(["\ufeff",result.fileContents], { type: 'text/csv' });
      a.href = URL.createObjectURL(file);
      a.download = 'test.csv';
      a.click();
      console.log(result);
      // saveAs(result);
    }, error => console.error(error));
  }
  b64DecodeUnicode(str) {
    return decodeURIComponent(Array.prototype.map.call(atob(str), function (c) {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
    }).join(''))
  }
}



interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

interface Stock{
  Title: string;
  ClosingPrice: number;
}
