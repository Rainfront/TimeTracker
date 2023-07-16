import {Button, Card, Form, FormText} from "react-bootstrap";
import {Error} from "./Error";
import React, {FormEvent, useState} from "react";
import {RequestPasswordReset} from "../Redux/Requests/UserRequests";

function RequestResetPassword(){
    const [loginOrEmail, setLoginOrEmail] = useState("");
    const [isSubmited, setIsSubmited] = useState(false);
    const onTextChange = (e: React.ChangeEvent<any>)=>{
        setLoginOrEmail(e.target.value);
    }
    const onSubmitHandler = (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        RequestPasswordReset(loginOrEmail).subscribe((x)=>{});
        setIsSubmited(true);
    }
    return (
        <>
            <div className="d-flex align-items-center flex-column m-1 ">
                <h5>Request password reset</h5>
                <Card style={{width: '18rem'}} className='d-flex align-items-center flex-column'>
                    <Card.Body className='p-3 w-100'>
                        <Form className="d-flex align-items-start flex-column"
                              onSubmit={(event) => onSubmitHandler(event)}>
                            <p className='m-0'>Login or Email</p>
                            <Form.Control
                                type="text"
                                className="w-100 mb-3"
                                onChange={onTextChange}
                            />
                            <Button
                                type="submit"
                                className="btn-success w-100"
                            >
                                Reset password
                            </Button>
                        </Form>
                        {isSubmited ? <FormText>Request has been sent</FormText>:<></>}
                    </Card.Body>
                </Card>
            </div>
        </>
    )
}
export default RequestResetPassword;