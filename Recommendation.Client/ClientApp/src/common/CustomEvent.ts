type EventCallback<TEventArgs> = (sender: object, args: TEventArgs) => boolean;

export class CustomEvent<TEventArgs = any> {
  private _callbackList = new Array<EventCallback<TEventArgs>>();

  public notify(sender: object, args: TEventArgs) {
    let allSuccessfull = true;
    for (const callback of this._callbackList) {
      allSuccessfull = allSuccessfull && callback(sender, args);
    }
    return allSuccessfull;
  }

  public register(callback: EventCallback<TEventArgs>) {
    this._callbackList.push(callback);
  }

  public unregister(callback: EventCallback<TEventArgs>) {
    this._callbackList.splice(this._callbackList.indexOf(callback), 1);
  }
}