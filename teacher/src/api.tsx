import axios, {AxiosResponse} from "axios";
import config from './config.json';

const getHeaders = () => {
    let headers : any = {
        "Content-Type": "application/json"
    };
    return headers;
};

export default {
    getAnswers(quizId : string) : Promise<AxiosResponse<Answer[]>> {
        return axios.request({
            method: 'GET',
            url: `${config.serviceBaseUrl}/${quizId}`,
            headers: getHeaders()
        });
    }
};