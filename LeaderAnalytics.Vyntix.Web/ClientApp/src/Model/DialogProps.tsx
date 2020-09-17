import DialogType from './DialogType';

export default class DialogProps {
    public Message: string = "";
    public DialogType: DialogType = DialogType.Info;
    public Callback: any = () => { };

    constructor(message: string, dialogType: DialogType, callBack: Function) {
        this.Message = message;
        this.DialogType = dialogType;
        this.Callback = callBack;
    }
}
